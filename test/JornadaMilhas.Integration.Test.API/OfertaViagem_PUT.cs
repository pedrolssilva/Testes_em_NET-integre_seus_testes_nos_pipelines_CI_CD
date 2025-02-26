using JornadaMilhas.Dominio.Entidades;
using JornadaMilhas.Dominio.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace JornadaMilhas.Integration.Test.API;
public class OfertaViagem_PUT : IClassFixture<JornadaMilhasWebApplicationFactory>
{
    private readonly JornadaMilhasWebApplicationFactory app;

    public OfertaViagem_PUT(JornadaMilhasWebApplicationFactory app)
    {
        this.app = app;
    }

    [Fact]
    public async Task Atualizar_OfertaViagem()
    {
        //Arrange  
        app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");
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

        ofertaExistente.Rota.Origem = "Origem Atualizada";
        ofertaExistente.Rota.Destino = "Destino Atualizada";

        using var client = await app.GetClientWithAccessTokenAsync();

        //Act
        var response = await client.PutAsJsonAsync($"/ofertas-viagem/", ofertaExistente);

        //Assert
        Assert.True(response != null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //Ajustes
    }
}
