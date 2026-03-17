using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;

namespace BackendTests.UnitTests.Application.DTOs;

public class PessoaValidationUnitTests
{
    [Fact(DisplayName = "Validação deve falhar se data de nascimento for no futuro")]
    public void ValidarDataNascimento_DataFutura_DeveRetornarErro()
    {
        var dataFutura = DateTime.Today.AddDays(1);
        var context = new ValidationContext(new Object());

        var result = PessoaValidation.ValidarDataNascimento(dataFutura, context);

        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("Data de nascimento não pode ser no futuro.");
    }

    [Theory(DisplayName = "Validação deve passar se data de nascimento for hoje ou no passado")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10000)]
    public void ValidarDataNascimento_DataPassadaOuHoje_DeveRetornarSucesso(int diasParaAdicionar)
    {
        var data = DateTime.Today.AddDays(diasParaAdicionar);
        var context = new ValidationContext(new Object());

        var result = PessoaValidation.ValidarDataNascimento(data, context);

        result.Should().Be(ValidationResult.Success);
    }
}