# BUG-003: Transações excluídas em cascata continuam visíveis na listagem devido a cache

## Descrição
Ao excluir uma Pessoa que possui transações vinculadas, o backend realiza a exclusão em cascata corretamente. Porém, ao navegar para a tela de "Transações" logo em seguida, as transações da pessoa excluída continuam aparecendo na listagem. O usuário precisa atualizar a página manualmente (F5) para que elas sumam.

## Pré-condições
- Uma Pessoa cadastrada.
- Uma Transação vinculada a essa Pessoa.

## Passos para Reproduzir
1. Acesse a tela "Pessoas".
2. Exclua a pessoa que possui a transação.
3. Clique no menu lateral "Transações".
4. Observe a listagem de transações.

## Resultado Esperado
A listagem de transações deveria ser atualizada automaticamente, não exibindo mais as transações da pessoa excluída.

## Resultado Atual
A transação continua visível na tabela. Ela só desaparece se a página for recarregada.

## Severidade
**Baixa/Média**. Trata-se de um problema de UX e inconsistência de estado na interface. Os dados no banco de dados estão corretos, mas o usuário visualiza informações fantasma.

## Análise Técnica (Causa Raiz)
No frontend, o hook `useDeletePessoa` (em `usePessoas.ts`) invalida apenas o cache da query `"pessoas"` após o sucesso da mutação. Como as queries têm um `staleTime` de 5 minutos configurado no `ReactQueryProvider`, a navegação para a rota de transações exibe os dados cacheados. A mutação de exclusão de pessoa também deveria invalidar o cache de `"transacoes"` e `"totais-pessoas"`.