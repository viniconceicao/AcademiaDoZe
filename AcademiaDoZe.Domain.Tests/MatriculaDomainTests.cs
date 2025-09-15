// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests
{
    public class MatriculaDomainTests
    {
        private Logradouro GetValidLogradouro() =>
            Logradouro.Criar(0, "12345678", "Rua A", "Centro", "Cidade", "SP", "Brasil");

        private Arquivo GetValidArquivo() =>
            Arquivo.Criar(new byte[1], ".pdf");

        private Aluno GetAlunoValido(DateOnly nascimento) =>
            Aluno.Criar(
                "Fernanda Souza",
                "12345678900",
                nascimento,
                "11999999999",
                "fernanda@email.com",
                GetValidLogradouro(),
                "100",
                "Apto 2",
                "Senha@123",
                GetValidArquivo()
            );

        private DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

        [Fact]
        public void CriarMatricula_Valida_DeveCriarObjeto()
        {
            var aluno = GetAlunoValido(Hoje.AddYears(-20));
            var dataInicio = Hoje;
            var dataFim = dataInicio.AddMonths(6);

            var matricula = Matricula.Criar(
                aluno,
                EMatriculaPlano.Trimestral,
                dataInicio,
                dataFim,
                "Melhorar condicionamento",
                EMatriculaRestricoes.None,
                GetValidArquivo() // substitui null por um Arquivo válido
            );

            Assert.NotNull(matricula);
            Assert.Equal(aluno, matricula.AlunoMatricula);
            Assert.Equal("Melhorar condicionamento", matricula.Objetivo);
        }

        [Fact]
        public void CriarMatricula_SemAluno_DeveLancarExcecao()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Matricula.Criar(
                    null!,
                    EMatriculaPlano.Mensal,
                    Hoje,
                    Hoje.AddMonths(1),
                    "Saúde",
                    EMatriculaRestricoes.None,
                    GetValidArquivo()
                )
            );

            Assert.Equal("ALUNO_INVALIDO", ex.Message);
        }

        [Fact]
        public void CriarMatricula_AlunoMenor16SemLaudo_DeveLancarExcecao()
        {
            var alunoMenor = GetAlunoValido(Hoje.AddYears(-15));

            var ex = Assert.Throws<DomainException>(() =>
                Matricula.Criar(
                    alunoMenor,
                    EMatriculaPlano.Semestral,
                    Hoje,
                    Hoje.AddMonths(6),
                    "Força",
                    EMatriculaRestricoes.None,
                    GetValidArquivo()
                )
            );

            Assert.Equal("MENOR16_LAUDO_OBRIGATORIO", ex.Message);
        }

        [Fact]
        public void CriarMatricula_ComPlanoInvalido_DeveLancarExcecao()
        {
            var aluno = GetAlunoValido(Hoje.AddYears(-20));

            var planoInvalido = (EMatriculaPlano)999;

            var ex = Assert.Throws<DomainException>(() =>
                Matricula.Criar(
                    aluno,
                    planoInvalido,
                    Hoje,
                    Hoje.AddMonths(1),
                    "Objetivo qualquer",
                    EMatriculaRestricoes.None,
                    GetValidArquivo()
                )
            );

            Assert.Equal("PLANO_INVALIDO", ex.Message);
        }

        [Fact]
        public void CriarMatricula_SemDataInicio_DeveLancarExcecao()
        {
            var aluno = GetAlunoValido(Hoje.AddYears(-20));

            var ex = Assert.Throws<DomainException>(() =>
                Matricula.Criar(
                    aluno,
                    EMatriculaPlano.Anual,
                    default,
                    Hoje.AddMonths(12),
                    "Perder peso",
                    EMatriculaRestricoes.None,
                    GetValidArquivo()
                )
            );

            Assert.Equal("DATA_INICIO_OBRIGATORIO", ex.Message);
        }

        [Fact]
        public void CriarMatricula_SemObjetivo_DeveLancarExcecao()
        {
            var aluno = GetAlunoValido(Hoje.AddYears(-20));

            var ex = Assert.Throws<DomainException>(() =>
                Matricula.Criar(
                    aluno,
                    EMatriculaPlano.Mensal,
                    Hoje,
                    Hoje.AddMonths(1),
                    "",
                    EMatriculaRestricoes.None,
                    GetValidArquivo()
                )
            );

            Assert.Equal("OBJETIVO_OBRIGATORIO", ex.Message);
        }

        [Fact]
        public void CriarMatricula_ComRestricaoSemLaudo_DeveLancarExcecao()
        {
            var aluno = GetAlunoValido(Hoje.AddYears(-20));

            var ex = Assert.Throws<DomainException>(() =>
                Matricula.Criar(
                    aluno,
                    EMatriculaPlano.Mensal,
                    Hoje,
                    Hoje.AddMonths(1),
                    "Condicionamento",
                    EMatriculaRestricoes.Diabetes,
                    GetValidArquivo()
                )
            );

            Assert.Equal("RESTRICOES_LAUDO_OBRIGATORIO", ex.Message);
        }

        [Fact]
        public void CriarMatricula_ComRestricaoComLaudo_DeveCriarComSucesso()
        {
            var aluno = GetAlunoValido(Hoje.AddYears(-20));
            var laudo = GetValidArquivo();

            var matricula = Matricula.Criar(
                aluno,
                EMatriculaPlano.Semestral,
                Hoje,
                Hoje.AddMonths(6),
                "Treino com acompanhamento",
                EMatriculaRestricoes.Diabetes,
                laudo,
                "Problema no joelho"
            );

            Assert.NotNull(matricula);
            Assert.Equal("Problema no joelho", matricula.ObservacoesRestricoes);
        }
    }
}
