using System.Net;
using System.Net.Http.Json;
using System.Runtime;
using BackendTests.Config;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;

namespace BackendTests;

public class TransacoesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransacoesIntegrationTests(CustomWebApplicationFactory factory)
    {
        var clientOptions = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        };
        _client = factory.CreateClient(clientOptions);
    }

    [Fact(DisplayName = "Deve impedir o cadastro de receita para pessoa menor de idade")]
    [Trait("Category", "Bug")]
    [Trait("Issue", "BUG-001")]
    public async Task DeveImpedir_CadastroDeReceita_ParaMenorDeIdade()
    {
        var pessoaDto = new CreatePessoaDto
        {
            Nome = "Criança Teste",
            DataNascimento = DateTime.Today.AddYears(-10)
        };
        var pessoaResponse = await _client.PostAsJsonAsync("/api/v1/pessoas", pessoaDto);

        var pessoaErro = await pessoaResponse.Content.ReadAsStringAsync();
        pessoaResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"A API falhou ao criar a pessoa. Detalhes: {pessoaErro}");
        
        var pessoa = await pessoaResponse.Content.ReadFromJsonAsync<PessoaDto>();

        var categoriaDto = new CreateCategoriaDto
        {
            Descricao = "Mesada",
            Finalidade = Categoria.EFinalidade.Receita
        };
        var categoriaResponse = await _client.PostAsJsonAsync("/api/v1/categorias", categoriaDto);
        
        var categoriaErro = await categoriaResponse.Content.ReadAsStringAsync();
        categoriaResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"A API falhou ao criar a categoria. Detalhes: {categoriaErro}");
        
        var categoria = await categoriaResponse.Content.ReadFromJsonAsync<CategoriaDto>();

        var novaTransacao = new CreateTransacaoDto
        {
            Descricao = "Dinheiro da Vó",
            Valor = 50.0m,
            Tipo = Transacao.ETipo.Receita,
            CategoriaId = categoria!.Id,
            PessoaId = pessoa!.Id,
            Data = DateTime.Today
        };

        var transacaoResponse = await _client.PostAsJsonAsync("/api/v1/transacoes", novaTransacao);

        var transacaoResponseBody = await transacaoResponse.Content.ReadAsStringAsync();
        
        transacaoResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"BUG-001: Esperado bloqueio (400) da regra de negócio, mas a API retornou: {transacaoResponse.StatusCode} - {transacaoResponseBody}");

        transacaoResponseBody.Should().Contain("Menores de 18 anos não podem registrar receitas");
    }
}