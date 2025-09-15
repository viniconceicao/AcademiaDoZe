// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests
{
    public class MatriculaInfrastructureTests : TestBase
    {
        private async Task<Aluno> CriarAlunoParaMatricula()
        {
            var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);
            var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);

            var endereco = Logradouro.Criar(
                id: 0,
                cep: $"{Random.Shared.Next(10000, 99999)}-{Random.Shared.Next(100, 999)}",
                nome: "Rua Teste",
                bairro: "Centro",
                cidade: "Cidade Teste",
                estado: "ST",
                pais: "Brasil"
            );

            // Salva o logradouro e obtém o objeto com Id preenchido
            var enderecoSalvo = await repoLogradouro.Adicionar(endereco);

            var foto = Arquivo.Criar(
                conteudo: new byte[] { 1, 2, 3, 4 },
                tipoArquivo: "png"
            );

            var aluno = Aluno.Criar(
                nome: "Aluno Teste",
                cpf: $"{Random.Shared.NextInt64(10000000000, 99999999999)}",
                dataNascimento: new DateOnly(2000, 1, 1),
                telefone: "11999999999",
                email: "aluno@teste.com",
                endereco: enderecoSalvo, // Usa o logradouro já salvo
                numero: "123",
                complemento: "Apto 101",
                senha: "Senha123",
                foto: foto
            );

            return await repoAluno.Adicionar(aluno);
        }

        private Matricula CriarMatricula(Aluno aluno, int diasValidade = 30)
        {
            var laudo = Arquivo.Criar(
                conteudo: new byte[] { 5, 6, 7, 8 },
                tipoArquivo: "pdf"
            );

            return Matricula.Criar(
                alunoMatricula: aluno,
                plano: EMatriculaPlano.Mensal,
                dataInicio: DateOnly.FromDateTime(DateTime.Today),
                dataFim: DateOnly.FromDateTime(DateTime.Today.AddDays(diasValidade)),
                objetivo: "Ganho de massa",
                restricoesMedicas: EMatriculaRestricoes.None,
                laudoMedico: laudo,
                observacoesRestricoes: string.Empty
            );
        }

        [Fact]
        public async Task Deve_Adicionar_Matricula()
        {
            var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

            var aluno = await CriarAlunoParaMatricula();
            var matricula = CriarMatricula(aluno);

            var inserida = await repoMatricula.Adicionar(matricula);

            Assert.NotNull(inserida);
            Assert.True(inserida.Id > 0);
            Assert.Equal(aluno.Id, inserida.AlunoMatricula.Id);
        }

        [Fact]
        public async Task Deve_Atualizar_Matricula()
        {
            var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

            var aluno = await CriarAlunoParaMatricula();
            var matricula = await repoMatricula.Adicionar(CriarMatricula(aluno));

            // Como não é possível alterar diretamente a propriedade Objetivo (set é inacessível),
            // crie uma nova instância de Matricula com o novo objetivo e o mesmo Id
            var matriculaAtualizada = Matricula.Criar(
                alunoMatricula: matricula.AlunoMatricula,
                plano: matricula.Plano,
                dataInicio: matricula.DataInicio,
                dataFim: matricula.DataFim,
                objetivo: "Definição muscular",
                restricoesMedicas: matricula.RestricoesMedicas,
                laudoMedico: matricula.LaudoMedico,
                observacoesRestricoes: matricula.ObservacoesRestricoes
            );
            // Preserva o Id da matrícula original
            typeof(Matricula).GetProperty("Id")!.SetValue(matriculaAtualizada, matricula.Id);

            var atualizada = await repoMatricula.Atualizar(matriculaAtualizada);

            Assert.Equal("Definição muscular", atualizada.Objetivo);
        }

        [Fact]
        public async Task Deve_Obter_Matriculas_Por_Aluno()
        {
            var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

            var aluno = await CriarAlunoParaMatricula();
            await repoMatricula.Adicionar(CriarMatricula(aluno));

            var matriculas = await repoMatricula.ObterPorAluno(aluno.Id);

            Assert.NotEmpty(matriculas);
            Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoMatricula.Id));
        }

        [Fact]
        public async Task Deve_Obter_Matriculas_Ativas()
        {
            var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

            var aluno = await CriarAlunoParaMatricula();
            await repoMatricula.Adicionar(CriarMatricula(aluno, diasValidade: 10));

            var ativas = await repoMatricula.ObterAtivas(aluno.Id);

            Assert.NotEmpty(ativas);
            Assert.All(ativas, m => Assert.True(m.DataFim >= DateOnly.FromDateTime(DateTime.Today)));
        }

        [Fact]
        public async Task Deve_Obter_Matriculas_Vencendo_Em_7_Dias()
        {
            var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

            var aluno = await CriarAlunoParaMatricula();
            await repoMatricula.Adicionar(CriarMatricula(aluno, diasValidade: 5));

            var vencendo = await repoMatricula.ObterVencendoEmDias(7);

            Assert.NotEmpty(vencendo);
            Assert.All(vencendo, m =>
                Assert.InRange(m.DataFim.ToDateTime(TimeOnly.MinValue), DateTime.Today, DateTime.Today.AddDays(7))
            );
        }

        [Fact]
        public async Task Deve_Remover_Matricula()
        {
            var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

            var aluno = await CriarAlunoParaMatricula();
            var matricula = await repoMatricula.Adicionar(CriarMatricula(aluno));

            var removida = await repoMatricula.Remover(matricula.Id);

            Assert.True(removida);

            var matriculaRemovida = await repoMatricula.ObterPorId(matricula.Id);
            Assert.Null(matriculaRemovida);
        }
    }
}
