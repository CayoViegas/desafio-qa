using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace BackendTests.UnitTests.Domain.Entities;

public class PessoaUnitTests
{
    [Fact(DisplayName = "Pessoa com exatamente 18 anos deve ser considerada maior de idade")]
    public void Pessoa_Exatamente18Anos_DeveSerMaiorDeIdade()
    {
        var pessoa = new Pessoa { DataNascimento = DateTime.Today.AddYears(-18) };

        pessoa.EhMaiorDeIdade().Should().BeTrue();
        pessoa.Idade.Should().Be(18);
    }

    [Fact(DisplayName = "Pessoa com 17 anos e 364 dias deve ser considerada menor de idade")]
    public void Pessoa_Quase18Anos_DeveSerMenorDeIdade()
    {
        var pessoa = new Pessoa { DataNascimento = DateTime.Today.AddYears(-18).AddDays(1) };

        pessoa.EhMaiorDeIdade().Should().BeFalse();
        pessoa.Idade.Should().Be(17);
    }
}