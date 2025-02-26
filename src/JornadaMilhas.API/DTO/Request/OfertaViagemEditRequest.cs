namespace JornadaMilhas.API.DTO.Request;

public record OfertaViagemEditRequest(int Id, RotaRequest rota, PeriodoRequest periodo, double preco) : OfertaViagemRequest(rota, periodo, preco);

