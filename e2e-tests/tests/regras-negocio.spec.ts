import { test, expect } from '@playwright/test';

test.describe('Regras de Negócio - Interface UI', () => {
    
    test.beforeEach(async ({ page }) => {
        await page.goto('http://localhost:5173/');
    });

    test('Deve bloquear cadastro de receita para menor de idade na interface', async ({ page }) => {
        const uniqueId = Date.now().toString();
        const nomePessoa = `Criança E2E ${uniqueId}`;
        const nomeCategoria = `Mesada E2E ${uniqueId}`;

        await page.locator('.sidebar').getByRole('link', { name: 'Pessoas' }).click();
        await page.getByRole('button', { name: 'Adicionar Pessoa' }).click();
        await page.getByLabel('Nome').fill(nomePessoa);
        await page.getByLabel('Data de Nascimento').fill('2019-01-01');
        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Pessoa salva com sucesso!')).toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Categorias' }).click();
        await page.getByRole('button', { name: 'Adicionar Categoria' }).click();
        await page.getByLabel('Descrição').fill(nomeCategoria);
        await page.getByLabel('Finalidade').selectOption('receita');
        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Categoria salva com sucesso!')).toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Transações' }).click();
        await page.getByRole('button', { name: 'Adicionar Transação' }).click();
        await page.getByLabel('Descrição').fill('Dinheiro do Lanche');
        await page.getByLabel('Data').fill('2026-01-01');
        await page.getByLabel('Valor').fill('50');
        await page.getByLabel('Tipo').selectOption('receita');

        await page.getByPlaceholder('Pesquisar pessoas...').fill(nomePessoa);
        await page.getByRole('option', { name: nomePessoa }).click();

        await page.getByPlaceholder('Pesquisar categorias...').fill(nomeCategoria);
        await page.getByRole('option', { name: nomeCategoria }).click();

        await page.getByRole('button', { name: 'Salvar' }).click();

        await expect(page.getByText('Menores de 18 anos não podem registrar receitas.')).toBeVisible();

        await page.getByRole('button', { name: 'Cancelar' }).click();
        await page.locator('.sidebar').getByRole('link', { name: 'Pessoas' }).click();
        const linhaDaCrianca = page.getByRole('row', { name: nomePessoa });
        await linhaDaCrianca.getByRole('button', { name: 'Deletar' }).click();
        await page.getByRole('button', { name: 'Confirmar' }).click();
        await expect(page.getByRole('dialog')).not.toBeVisible();
        await expect(linhaDaCrianca).not.toBeVisible();
    });

    test('Deve refletir a exclusão em cascata de transações ao deletar uma pessoa', async ({ page }) => {
        const uniqueId = Date.now().toString();
        const nomePessoa = `Sr. Cascata ${uniqueId}`;
        const nomeCategoria = `Despesa Fixa ${uniqueId}`;
        const descTransacao = `Conta de Água ${uniqueId}`;

        await page.locator('.sidebar').getByRole('link', { name: 'Pessoas' }).click();
        await page.getByRole('button', { name: 'Adicionar Pessoa' }).click();
        await page.getByLabel('Nome').fill(nomePessoa);
        await page.getByLabel('Data de Nascimento').fill('1990-01-01');
        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Pessoa salva com sucesso!')).toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Categorias' }).click();
        await page.getByRole('button', { name: 'Adicionar Categoria' }).click();
        await page.getByLabel('Descrição').fill(nomeCategoria);
        await page.getByLabel('Finalidade').selectOption('despesa');
        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Categoria salva com sucesso!')).toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Transações' }).click();
        await page.getByRole('button', { name: 'Adicionar Transação' }).click();
        await page.getByLabel('Descrição').fill(descTransacao);
        await page.getByLabel('Data').fill('2026-01-01');
        await page.getByLabel('Valor').fill('100');
        await page.getByLabel('Tipo').selectOption('despesa');

        await page.getByPlaceholder('Pesquisar pessoas...').fill(nomePessoa);
        await page.getByRole('option', { name: nomePessoa }).click();
        await page.getByPlaceholder('Pesquisar categorias...').fill(nomeCategoria);
        await page.getByRole('option', { name: nomeCategoria }).click();

        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Transação salva com sucesso!')).toBeVisible();
        await expect(page.getByRole('cell', { name: descTransacao })).toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Pessoas' }).click();
        const linhaDaPessoa = page.getByRole('row', { name: nomePessoa });
        await linhaDaPessoa.getByRole('button', { name: 'Deletar' }).click();
        await page.getByRole('button', { name: 'Confirmar' }).click();
        await expect(page.getByRole('dialog')).not.toBeVisible();
        await expect(linhaDaPessoa).not.toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Transações' }).click();
        await expect(page.getByRole('heading', { name: 'Transações' })).toBeVisible();
        // WORKAROUND: Força o reload da página para burlar o cache stale do frontend (BUG-003)
        // await page.reload();
        await expect(page.getByRole('cell', { name: descTransacao })).not.toBeVisible();
    });

    test('Deve bloquear transação com categoria de finalidade incompatível mostrando erro genérico (Reflexo do BUG-002)', async ({ page }) => {
        const uniqueId = Date.now().toString();
        const nomePessoa = `Adulto E2E ${uniqueId}`;
        const nomeCategoria = `Categoria de Despesa ${uniqueId}`;

        await page.locator('.sidebar').getByRole('link', { name: 'Pessoas' }).click();
        await page.getByRole('button', { name: 'Adicionar Pessoa' }).click();
        await page.getByLabel('Nome').fill(nomePessoa);
        await page.getByLabel('Data de Nascimento').fill('1990-01-01');
        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Pessoa salva com sucesso!')).toBeVisible();

        await page.locator('.sidebar').getByRole('link', { name: 'Categorias' }).click();
        await page.getByRole('button', { name: 'Adicionar Categoria' }).click();
        await page.getByLabel('Descrição').fill(nomeCategoria);
        await page.getByLabel('Finalidade').selectOption('despesa');
        await page.getByRole('button', { name: 'Salvar' }).click();
        await expect(page.getByText('Categoria salva com sucesso!')).toBeVisible();
 
        await page.locator('.sidebar').getByRole('link', { name: 'Transações' }).click();
        await page.getByRole('button', { name: 'Adicionar Transação' }).click();
        await page.getByLabel('Descrição').fill('Receita Indevida');
        await page.getByLabel('Data').fill('2026-01-01');
        await page.getByLabel('Valor').fill('500');    
    
        await page.getByLabel('Tipo').selectOption('receita');

        await page.getByPlaceholder('Pesquisar pessoas...').fill(nomePessoa);
        await page.getByRole('option', { name: nomePessoa }).click();

        await page.getByPlaceholder('Pesquisar categorias...').fill(nomeCategoria);
        await page.getByRole('option', { name: nomeCategoria }).click();

        await page.getByRole('button', { name: 'Salvar' }).click();

        // Devido ao BUG-002 do backend (retorna 500 em vez de 400), o frontend exibe a mensagem genérica
        await expect(page.getByText('Erro ao salvar transação. Tente novamente.')).toBeVisible();

        await page.getByRole('button', { name: 'Cancelar' }).click();
        await page.locator('.sidebar').getByRole('link', { name: 'Pessoas' }).click();
        const linhaDaPessoa = page.getByRole('row', { name: nomePessoa });
        await linhaDaPessoa.getByRole('button', { name: 'Deletar' }).click();
        await page.getByRole('button', { name: 'Confirmar' }).click();
        await expect(page.getByRole('dialog')).not.toBeVisible();
        await expect(linhaDaPessoa).not.toBeVisible();
    });

});