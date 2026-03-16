# BUG-002: Erro 500 ao tentar registrar transação com categoria incompatível

## Descrição
Ao tentar criar uma transação (ex: Receita) utilizando uma Categoria cuja finalidade seja estritamente oposta (ex: Despesa), a API bloqueia a criação (o que está correto conforme a regra de negócio), porém retorna o Status HTTP `500 Internal Server Error`, quando o correto seria retornar `400 Bad Request` ou `422 Unprocessable Entity`.

## Pré-condições
- A aplicação deve estar em execução.
- Deve existir uma Pessoa (maior de idade) registrada no sistema.
- Deve existir uma Categoria cadastrada com a finalidade "Despesa".

## Passos para Reproduzir
1. Realizar uma requisição POST para `/api/v1/transacoes` com o seguinte corpo:
```json
{
  "descricao": "Venda de Bicicleta",
  "valor": 150.0,
  "tipo": 1, 
  "categoriaId": "ID_DA_CATEGORIA_DE_DESPESA",
  "pessoaId": "ID_DA_PESSOA_ADULTA",
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
    "Detailed": "Não é possível registrar receita em categoria de despesa."
}
```

## Severidade
**Média**. A regra de negócio não é burlada (os dados não são salvos no banco), mas o retorno 500 indica falha de tratamento no backend, o que prejudica a integração com o frontend e a monitoria da aplicação.

## Análise Técnica (Causa Raiz)
A exceção `InvalidOperationException` é disparada corretamente na entidade `Transacao`, porém o `ExceptionMiddleware.cs` trata todas as exceções genéricas como `500 Internal Server Error`. Falta um tratamento específico no middleware ou no Controller para capturar as exceções de domínio e convertê-las no status adequado.