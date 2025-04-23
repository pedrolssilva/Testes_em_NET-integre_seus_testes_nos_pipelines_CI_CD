using JornadaMilhas.API.DTO.Response;
using JornadaMilhas.Dominio.Entidades;
using JornadaMilhas.Dominio.ValueObjects;
using JornadaMilhas.Integration.Test.API.DataBuilders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net.Http.Json;

namespace JornadaMilhas.Integration.Test.API;
public class OfertaViagem_GET : IClassFixture<JornadaMilhasWebApplicationFactory>
{
    private readonly JornadaMilhasWebApplicationFactory app;

    public OfertaViagem_GET(JornadaMilhasWebApplicationFactory app)
    {
        this.app = app;
    }


    [Fact]
    public async Task Recuperar_OfertaViagem_PorId()
    {
        //Arrange  
        var ofertaExistente = app.Context.OfertasViagem.FirstOrDefault();
        if (ofertaExistente is null)
        {
            ofertaExistente = new OfertaViagem()
            {
                Preco = 100,
                Rota = new Rota("Origem", "Destino"),
                Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
            };
            app.Context.Add(ofertaExistente);
            app.Context.SaveChanges();
        }

        using var client = await app.GetClientWithAccessTokenAsync();

        //Act
        var response = await client.GetFromJsonAsync<OfertaViagem>("/ofertas-viagem/" + ofertaExistente.Id);

        //Assert
        Assert.NotNull(response);
        Assert.Equal(ofertaExistente.Preco, response.Preco, 0.001);
        Assert.Equal(ofertaExistente.Rota.Origem, response.Rota.Origem);
        Assert.Equal(ofertaExistente.Rota.Destino, response.Rota.Destino);
    }

    [Fact]
    public async Task Recuperar_OfertasViagens_Na_Consulta_Paginada()
    {
        //Arrange
        OfertaViagemDataBuilder databuilder = new OfertaViagemDataBuilder();
        var listaOfertas = databuilder.Generate(80);
        app.Context.OfertasViagem.AddRange(listaOfertas);
        app.Context.SaveChanges();

        using var client = await app.GetClientWithAccessTokenAsync();

        int pagina = 1;
        int tamanhoPorPagina = 80;

        //Act
        var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem/?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

        //Assert
        Assert.True(response != null);
        Assert.Equal(tamanhoPorPagina, response.Count());
    }

    [Fact]
    public async Task Recuperar_OfertasViagens_Na_Ultima_Pagina()
    {
        //Arrange
        app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");
        OfertaViagemDataBuilder databuilder = new OfertaViagemDataBuilder();
        var listaOfertas = databuilder.Generate(80);
        app.Context.OfertasViagem.AddRange(listaOfertas);
        app.Context.SaveChanges();

        using var client = await app.GetClientWithAccessTokenAsync();

        int pagina = 4;
        int tamanhoPorPagina = 25;

        //Act
        var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem/?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

        //Assert
        Assert.True(response != null);
        // a expectativa de qtde retornada é a diferença entre tamanho da página e o resto de registros
        // tamanhoPorPagina = 25 - [(pagina * tamanhoPorPagina = 100) - (total de registros = 80)] == 5
        Assert.Equal(5, response.Count());
    }

    [Fact]
    public async Task Recuperar_OfertasViagens_Quando_Pagina_Nao_Existente()
    {
        //Arrange
        app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

        OfertaViagemDataBuilder databuilder = new OfertaViagemDataBuilder();

        var listaOfertas = databuilder.Generate(80);
        app.Context.OfertasViagem.AddRange(listaOfertas);
        app.Context.SaveChanges();

        using var client = await app.GetClientWithAccessTokenAsync();

        int pagina = 5;
        int tamanhoPorPagina = 25;

        //Act
        var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

        //Assert
        Assert.True(response != null);
        Assert.Empty(response);
    }

    [Fact]
    public async Task Recuperar_OfertasViagens_Quando_Pagina_Negativa()
    {
        //Arrange
        app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

        OfertaViagemDataBuilder databuilder = new OfertaViagemDataBuilder();

        var listaOfertas = databuilder.Generate(80);
        app.Context.OfertasViagem.AddRange(listaOfertas);
        app.Context.SaveChanges();

        using var client = await app.GetClientWithAccessTokenAsync();

        int pagina = -5;
        int tamanhoPorPagina = 25;

        //Act + Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {

            var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");
        });

    }

    [Fact]
    public async Task Recuperar_OfertasViagens_Com_Maior_Desconto()
    {
        // Arrange
        var ofertaExistente = app.Context.OfertasViagem.FirstOrDefault();
        if (ofertaExistente is null)
        {
            ofertaExistente = new OfertaViagem()
            {
                Preco = 100,
                Rota = new Rota("Origem", "Destino"),
                Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
            };
            app.Context.Add(ofertaExistente);
            app.Context.SaveChanges();
        }

        using var client = await app.GetClientWithAccessTokenAsync();

        // Act
        var response = await client.GetFromJsonAsync<OfertaViagem>($"/ofertas-viagem/maior-desconto");

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Recuperar_Ultima_OfertaViagem_Inserida()
    {
        // Arrange
        app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

        OfertaViagemDataBuilder databuilder = new OfertaViagemDataBuilder();

        var listaOfertas = databuilder.Generate(3);

        app.Context.OfertasViagem.AddRange(listaOfertas);
        app.Context.SaveChanges();

        var lastInserted = listaOfertas.Last();

        using var client = await app.GetClientWithAccessTokenAsync();

        // Act
        var response = await client.GetFromJsonAsync<OfertaViagemResponse>($"/ofertas-viagem/ultima-inserida");

        // Assert
        Assert.Equivalent(new
        {
            Id = lastInserted.Id,
            Rota = new { lastInserted.Rota.Id, lastInserted.Rota.Origem, lastInserted.Rota.Destino },
            Periodo = new { lastInserted.Periodo.DataInicial, lastInserted.Periodo.DataFinal },
            Preco = lastInserted.Preco
        },
            response, false);
    }

}
