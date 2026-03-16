# BUG-001: Erro 500 ao tentar cadastrar receita para menor de idade

## Descrição
Ao tentar criar uma transação do tipo "Receita" associada a uma Pessoa menor de 18 anos, a API bloqueia a criação (o que está correto conforme a regra de negócio), porém retorna o Status HTTP `500 Internal Server Error`, quando o correto seria retornar `400 Bad Request` ou `422 Unprocessable Entity`.

## Passos para Reproduzir
1. Realizar uma requisição POST para `/api/v1/transacoes` com o seguinte corpo:
```json
{
  "descricao": "Dinheiro da Vó",
  "valor": 50.0,
  "tipo": 1, 
  "categoriaId": "ID_DA_CATEGORIA_RECEITA",
  "pessoaId": "ID_DA_PESSOA_MENOR_DE_IDADE",
  "data": "2024-03-15"
}
```

## Resultado Esperado
A API deveria retornar um HTTP Status `400 Bad Request`, informando de forma tratada que a regra de negócio foi violada.

## Resultado Atual
A API retorna um HTTP Status `500 Internal Server Error` com o seguinte detalhe no corpo:
```json
{
    "StatusCode": 500,
    "Message": "Ocorreu um erro interno no servidor.",
    "Detailed": "Menores de 18 anos não podem registrar receitas."
}
```

## Severidade
**Média**. A regra de negócio não é burlada (os dados não são salvos no banco), mas o retorno 500 indica falha de tratamento no backend, o que prejudica a integração com o frontend e a monitoria da aplicação.

## Análise Técnica (Causa Raiz)
A exceção `InvalidOperationException` é disparada corretamente na entidade `Transacao` (linha 96), porém o `ExceptionMiddleware.cs` trata todas as exceções genéricas como `500 Internal Server Error`. Falta um tratamento específico no middleware ou no Controller para capturar as exceções de domínio e convertê-las no status adequado.