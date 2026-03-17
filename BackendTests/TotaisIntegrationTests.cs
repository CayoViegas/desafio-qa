using System.Net;
using System.Net.Http.Json;
using BackendTests.Config;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.ValueObjects;

namespace BackendTests;

public class TotaisIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TotaisIntegrationTests(CustomWebApplicationFactory factory)
    {
        var clientOptions = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        };
        _client = factory.CreateClient(clientOptions);
    }

    [Fact(DisplayName = "Deve calcular corretamente os totais de receitas, despesas e saldo por pessoa")]
    public async Task DeveCalcular_TotaisPorPessoa_Corretamente()
    {
        var pessoaDto = new CreatePessoaDto { Nome = "Investidor Teste", DataNascimento = DateTime.Today.AddYears(-30) };
        var pessoaResponse = await _client.PostAsJsonAsync("/api/v1/pessoas", pessoaDto);
        var pessoa = await pessoaResponse.Content.ReadFromJsonAsync<PessoaDto>();

        var categoriaReceitaDto = new CreateCategoriaDto { Descricao = "Salário", Finalidade = Categoria.EFinalidade.Receita };
        var categoriaReceitaResponse = await _client.PostAsJsonAsync("/api/v1/categorias", categoriaReceitaDto);
        var categoriaReceita = await categoriaReceitaResponse.Content.ReadFromJsonAsync<CategoriaDto>();

        var categoriaDespesaDto = new CreateCategoriaDto { Descricao = "Alimentação", Finalidade = Categoria.EFinalidade.Despesa };
        var categoriaDespesaResponse = await _client.PostAsJsonAsync("/api/v1/categorias", categoriaDespesaDto);
        var categoriaDespesa = await categoriaDespesaResponse.Content.ReadFromJsonAsync<CategoriaDto>();

        await _client.PostAsJsonAsync("/api/v1/transacoes", new CreateTransacaoDto
        {
            Descricao = "Salário Fixo", Valor = 2000.0m, Tipo = Transacao.ETipo.Receita, 
            CategoriaId = categoriaReceita!.Id, PessoaId = pessoa!.Id, Data = DateTime.Today
        });

        await _client.PostAsJsonAsync("/api/v1/transacoes", new CreateTransacaoDto
        {
            Descricao = "Bônus", Valor = 500.0m, Tipo = Transacao.ETipo.Receita, 
            CategoriaId = categoriaReceita.Id, PessoaId = pessoa.Id, Data = DateTime.Today
        });

        await _client.PostAsJsonAsync("/api/v1/transacoes", new CreateTransacaoDto
        {
            Descricao = "Supermercado", Valor = 800.0m, Tipo = Transacao.ETipo.Despesa, 
            CategoriaId = categoriaDespesa!.Id, PessoaId = pessoa.Id, Data = DateTime.Today
        });

        var totaisResponse = await _client.GetAsync($"/api/v1/totais/pessoas?Pessoa.Id={pessoa.Id}");

        totaisResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await totaisResponse.Content.ReadFromJsonAsync<PagedResult<TotalPorPessoa>>();

        resultado.Should().NotBeNull();
        resultado!.Items.Should().NotBeEmpty("A lista de totais não deveria estar vazia.");

        var totaisDaPessoa = resultado.Items.First(t => t.PessoaId == pessoa.Id);

        totaisDaPessoa.TotalReceitas.Should().Be(2500.0m);
        totaisDaPessoa.TotalDespesas.Should().Be(800.0m);
        totaisDaPessoa.Saldo.Should().Be(1700.0m);
    }
}