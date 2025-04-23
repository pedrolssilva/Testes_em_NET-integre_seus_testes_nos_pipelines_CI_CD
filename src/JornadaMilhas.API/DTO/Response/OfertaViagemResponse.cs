namespace JornadaMilhas.API.DTO.Response;

public record OfertaViagemResponse(int Id, RotaResponse Rota, PeriodoResponse Periodo, double Preco);

