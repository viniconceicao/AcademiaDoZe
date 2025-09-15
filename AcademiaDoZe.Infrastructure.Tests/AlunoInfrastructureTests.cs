// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests
{
    public class AlunoInfrastructureTests : TestBase
    {
        [Fact]
        public async Task Aluno_LogradouroPorId_CpfJaExiste_Adicionar()
        {
            // Arrange
            var logradouroId = 4;
            var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);
            var logradouro = await repoLogradouro.ObterPorId(logradouroId);
            Assert.NotNull(logradouro);

            Arquivo arquivo = Arquivo.Criar(new byte[] { 1, 2, 3 }, ".jpg");
            var cpf = "98765432111";
            var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

            // Remove aluno com o mesmo CPF se existir
            var alunoExistente = await repoAluno.ObterPorCpf(cpf);
            if (alunoExistente != null)
                await repoAluno.Remover(alunoExistente.Id);

            // Act
            var cpfExistente = await repoAluno.CpfJaExiste(cpf);
            Assert.False(cpfExistente, "CPF já existe no banco de dados.");

            var aluno = Aluno.Criar(
                "João da Silva",
                cpf,
                new DateOnly(2005, 1, 15),
                "48999998888",
                "joao.silva@gmail.com",
                logradouro!,
                "123",
                "apto 101",
                "Senha123",
                arquivo
            );

            var alunoInserido = await repoAluno.Adicionar(aluno);

            // Assert
            Assert.NotNull(alunoInserido);
            Assert.True(alunoInserido.Id > 0);
        }

        [Fact]
        public async Task Aluno_ObterPorCpf_Atualizar()
        {
            // Arrange
            var cpf = "98765432100";
            var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

            // Remove e cria novo aluno para garantir ambiente limpo
            var alunoExistente = await repoAluno.ObterPorCpf(cpf);
            if (alunoExistente != null)
                await repoAluno.Remover(alunoExistente.Id);

            var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);
            var logradouro = await repoLogradouro.ObterPorId(4);
            Assert.NotNull(logradouro);

            Arquivo arquivoNovo = Arquivo.Criar(new byte[] { 1, 2, 3 }, ".jpg");
            var novoAluno = Aluno.Criar(
                "João da Silva",
                cpf,
                new DateOnly(2005, 1, 15),
                "48999998888",
                "joao.silva@gmail.com",
                logradouro!,
                "123",
                "apto 101",
                "Senha123",
                arquivoNovo
            );
            var alunoAdicionado = await repoAluno.Adicionar(novoAluno);
            Assert.NotNull(alunoAdicionado);

            Arquivo arquivo = Arquivo.Criar(new byte[] { 9, 8, 7 }, ".jpg");

            var alunoAtualizado = Aluno.Criar(
                "João Atualizado",
                alunoAdicionado.Cpf,
                alunoAdicionado.DataNascimento,
                alunoAdicionado.Telefone,
                alunoAdicionado.Email,
                alunoAdicionado.Endereco,
                alunoAdicionado.Numero,
                alunoAdicionado.Complemento,
                alunoAdicionado.Senha,
                arquivo
            );

            typeof(Entity).GetProperty("Id")?.SetValue(alunoAtualizado, alunoAdicionado.Id);

            // Act
            var resultadoAtualizacao = await repoAluno.Atualizar(alunoAtualizado);

            // Assert
            Assert.NotNull(resultadoAtualizacao);
            Assert.Equal("João Atualizado", resultadoAtualizacao.Nome);
        }

        [Fact]
        public async Task Aluno_ObterPorCpf_TrocarSenha()
        {
            // Arrange
            var cpf = "98765432100";
            var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

            // Remove e cria novo aluno para garantir ambiente limpo
            var alunoExistente = await repoAluno.ObterPorCpf(cpf);
            if (alunoExistente != null)
                await repoAluno.Remover(alunoExistente.Id);

            var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);
            var logradouro = await repoLogradouro.ObterPorId(4);
            Assert.NotNull(logradouro);

            Arquivo arquivoNovo = Arquivo.Criar(new byte[] { 1, 2, 3 }, ".jpg");
            var novoAluno = Aluno.Criar(
                "João da Silva",
                cpf,
                new DateOnly(2005, 1, 15),
                "48999998888",
                "joao.silva@gmail.com",
                logradouro!,
                "123",
                "apto 101",
                "Senha123",
                arquivoNovo
            );
            var alunoAdicionado = await repoAluno.Adicionar(novoAluno);
            Assert.NotNull(alunoAdicionado);

            var novaSenha = "novaSenhaAluno";

            // Act
            var resultadoTrocaSenha = await repoAluno.TrocarSenha(alunoAdicionado.Id, novaSenha);

            // Assert
            Assert.True(resultadoTrocaSenha);

            var alunoAtualizado = await repoAluno.ObterPorId(alunoAdicionado.Id);
            Assert.NotNull(alunoAtualizado);
            Assert.Equal(novaSenha, alunoAtualizado!.Senha);
        }

        [Fact]
        public async Task Aluno_ObterPorCpf_Remover_ObterPorId()
        {
            // Arrange
            var cpf = "98765432100";
            var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

            // Remove e cria novo aluno para garantir ambiente limpo
            var alunoExistente = await repoAluno.ObterPorCpf(cpf);
            if (alunoExistente != null)
                await repoAluno.Remover(alunoExistente.Id);

            var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);
            var logradouro = await repoLogradouro.ObterPorId(4);
            Assert.NotNull(logradouro);

            Arquivo arquivoNovo = Arquivo.Criar(new byte[] { 1, 2, 3 }, ".jpg");
            var novoAluno = Aluno.Criar(
                "João da Silva",
                cpf,
                new DateOnly(2005, 1, 15),
                "48999998888",
                "joao.silva@gmail.com",
                logradouro!,
                "123",
                "apto 101",
                "Senha123",
                arquivoNovo
            );
            var alunoAdicionado = await repoAluno.Adicionar(novoAluno);
            Assert.NotNull(alunoAdicionado);

            // Act
            var resultadoRemover = await repoAluno.Remover(alunoAdicionado.Id);

            // Assert
            Assert.True(resultadoRemover);

            var resultadoRemovido = await repoAluno.ObterPorId(alunoAdicionado.Id);
            Assert.Null(resultadoRemovido);
        }

        [Fact]
        public async Task Aluno_ObterTodos()
        {
            // Arrange
            var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

            // Act
            var resultado = (await repoAluno.ObterTodos()).ToList();

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Any());
        }
    }
}