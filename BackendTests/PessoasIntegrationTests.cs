using System.Net;
using System.Net.Http.Json;
using BackendTests.Config;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;

namespace BackendTests;

public class PessoasIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PessoasIntegrationTests(CustomWebApplicationFactory factory)
    {
        var clientOptions = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        };
        _client = factory.CreateClient(clientOptions);
    }

    [Fact(DisplayName = "Deve excluir transações em cascata ao excluir uma pessoa")]
    public async Task DeveExcluir_TransacoesEmCascata_AoExcluirPessoa()
    {
        var pessoaDto = new CreatePessoaDto
        {
            Nome = "Pessoa a ser excluída",
            DataNascimento = DateTime.Today.AddYears(-25)
        };
        var pessoaResponse = await _client.PostAsJsonAsync("/api/v1/pessoas", pessoaDto);
        
        var pessoaErro = await pessoaResponse.Content.ReadAsStringAsync();
        pessoaResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"A API falhou ao criar a pessoa. Detalhes: {pessoaErro}");
        
        var pessoa = await pessoaResponse.Content.ReadFromJsonAsync<PessoaDto>();

        var categoriaDto = new CreateCategoriaDto
        {
            Descricao = "Alimentação",
            Finalidade = Categoria.EFinalidade.Despesa
        };
        var categoriaResponse = await _client.PostAsJsonAsync("/api/v1/categorias", categoriaDto);
        
        var categoriaErro = await categoriaResponse.Content.ReadAsStringAsync();
        categoriaResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"A API falhou ao criar a categoria. Detalhes: {categoriaErro}");
        
        var categoria = await categoriaResponse.Content.ReadFromJsonAsync<CategoriaDto>();

        var transacaoDto = new CreateTransacaoDto
        {
            Descricao = "Almoço",
            Valor = 30.0m,
            Tipo = Transacao.ETipo.Despesa,
            CategoriaId = categoria!.Id,
            PessoaId = pessoa!.Id,
            Data = DateTime.Today
        };
        var transacaoResponse = await _client.PostAsJsonAsync("/api/v1/transacoes", transacaoDto);
        
        var transacaoErro = await transacaoResponse.Content.ReadAsStringAsync();
        transacaoResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"A API falhou ao criar a transação. Detalhes: {transacaoErro}");
        
        var transacao = await transacaoResponse.Content.ReadFromJsonAsync<TransacaoDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/pessoas/{pessoa!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getTransacaoResponse = await _client.GetAsync($"/api/v1/transacoes/{transacao!.Id}");

        getTransacaoResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "A transação deveria ter sido excluída em cascata junto com a pessoa, mas ainda foi encontrada.");
    }
}