using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace BackendTests.UnitTests.Domain.Entities;

public class CategoriaUnitTests
{
    [Theory(DisplayName = "Categoria deve validar permissão de tipo de transação corretamente")]
    [InlineData(Categoria.EFinalidade.Despesa, Transacao.ETipo.Despesa, true)]
    [InlineData(Categoria.EFinalidade.Despesa, Transacao.ETipo.Receita, false)]
    [InlineData(Categoria.EFinalidade.Receita, Transacao.ETipo.Receita, true)]
    [InlineData(Categoria.EFinalidade.Receita, Transacao.ETipo.Despesa, false)]
    [InlineData(Categoria.EFinalidade.Ambas, Transacao.ETipo.Receita, true)]
    [InlineData(Categoria.EFinalidade.Ambas, Transacao.ETipo.Despesa, true)]
    public void Categoria_PermiteTipo_DeveRetornarEsperado(Categoria.EFinalidade finalidade, Transacao.ETipo tipo, bool esperado)
    {
        var categoria = new Categoria { Finalidade = finalidade };

        var result = categoria.PermiteTipo(tipo);

        result.Should().Be(esperado);
    }
}