using System.Reflection;
using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace BackendTests.UnitTests.Domain.Entities;

public class TransacaoUnitTests
{
    private void SetInternalProperty(object target, string propertyName, object value)
    {
        try
        {
            var property = target.GetType().GetProperty(propertyName);
            property!.SetValue(target, value);
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException != null) throw ex.InnerException;
            throw;
        }
    }

    [Fact(DisplayName = "Pessoa menor de idade deve lançar exceção ao associar Receita na Transação")]
    public void Transacao_AssociarPessoa_MenorDeIdade_ComReceita_DeveLancarExcecao()
    {
        var pessoaMenor = new Pessoa
        {
            Nome = "Criança",
            DataNascimento = DateTime.Today.AddYears(-10)
        };

        var transacaoReceita = new Transacao
        {
            Descricao = "Mesada",
            Valor = 100m,
            Tipo = Transacao.ETipo.Receita,
            Data = DateTime.Today
        };

        Action act = () => SetInternalProperty(transacaoReceita, "Pessoa", pessoaMenor);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Menores de 18 anos não podem registrar receitas.");
    }

    [Fact(DisplayName = "Pessoa maior de idade pode associar Receita na Transação sem erro")]
    public void Transacao_AssociarPessoa_MaiorDeIdade_ComReceita_DeveAtribuirComSucesso()
    {
        var pessoaMaior = new Pessoa
        {
            Nome = "Adulto",
            DataNascimento = DateTime.Today.AddYears(-30)
        };

        var transacaoReceita = new Transacao
        {
            Descricao = "Salário",
            Valor = 3000m,
            Tipo = Transacao.ETipo.Receita,
            Data = DateTime.Today
        };

        Action act = () => SetInternalProperty(transacaoReceita, "Pessoa", pessoaMaior);

        act.Should().NotThrow();
        transacaoReceita.PessoaId.Should().Be(pessoaMaior.Id);
    }

    [Fact(DisplayName = "Transação de Receita em Categoria de Despesa deve lançar exceção")]
    public void Transacao_AssociarCategoria_ReceitaEmDespesa_DeveLancarExcecao()
    {
        var categoriaDespesa = new Categoria
        {
            Descricao = "Contas",
            Finalidade = Categoria.EFinalidade.Despesa
        };

        var transacaoReceita = new Transacao
        {
            Descricao = "Venda indevida",
            Valor = 100m,
            Tipo = Transacao.ETipo.Receita,
            Data = DateTime.Today
        };

        Action act = () => SetInternalProperty(transacaoReceita, "Categoria", categoriaDespesa);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível registrar receita em categoria de despesa.");
    }

    [Theory(DisplayName = "Transação pode ser associada a Categoria com finalidade compatível")]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Receita)]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Ambas)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Ambas)]
    public void Transacao_AssociarCategoria_Compativel_DeveAtribuirComSucesso(Transacao.ETipo tipoTransacao, Categoria.EFinalidade finalidadeCategoria)
    {
        var categoria = new Categoria
        {
            Descricao = "Categoria Compatível",
            Finalidade = finalidadeCategoria
        };

        var transacao = new Transacao
        {
            Descricao = "Teste Compatibilidade",
            Valor = 50m,
            Tipo = tipoTransacao,
            Data = DateTime.Today
        };

        Action act = () => SetInternalProperty(transacao, "Categoria", categoria);

        act.Should().NotThrow();
        transacao.CategoriaId.Should().Be(categoria.Id);
    }
}