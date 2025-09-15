// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests
{
    public class ColaboradorDomainTests
    {
        private Logradouro GetValidLogradouro() =>
            Logradouro.Criar(0, "12345678", "Rua A", "Centro", "Cidade", "SP", "Brasil");

        private Arquivo GetValidArquivo() =>
            Arquivo.Criar(new byte[1], ".jpg");

        private DateOnly GetValidNascimento() =>
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25));

        private DateOnly GetValidAdmissao() =>
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1));

        [Fact]
        public void CriarColaborador_Valido_DeveCriarObjeto()
        {
            var colaborador = Colaborador.Criar(
                "Ana Costa",
                "12345678900",
                GetValidNascimento(),
                "11999999999",
                "ana@email.com",
                GetValidLogradouro(),
                "100",
                "Sala 1",
                "Senha@123",
                GetValidArquivo(),
                GetValidAdmissao(),
                EColaboradorTipo.Colaborador,
                EColaboradorVinculo.Estagio
            );

            Assert.NotNull(colaborador);
            Assert.Equal("Ana Costa", colaborador.Nome);
        }

        [Fact]
        public void CriarColaborador_ComNomeVazio_DeveLancarExcecao()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Colaborador.Criar(
                    "",
                    "12345678900",
                    GetValidNascimento(),
                    "11999999999",
                    "teste@email.com",
                    GetValidLogradouro(),
                    "100",
                    "Sala 1",
                    "Senha@123",
                    GetValidArquivo(),
                    GetValidAdmissao(),
                    EColaboradorTipo.Colaborador,
                    EColaboradorVinculo.Estagio
                )
            );
            Assert.Equal("NOME_OBRIGATORIO", ex.Message);
        }

        [Fact]
        public void CriarColaborador_ComCpfInvalido_DeveLancarExcecao()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Colaborador.Criar(
                    "Lucas",
                    "123",
                    GetValidNascimento(),
                    "11999999999",
                    "lucas@email.com",
                    GetValidLogradouro(),
                    "100",
                    "Sala 1",
                    "Senha@123",
                    GetValidArquivo(),
                    GetValidAdmissao(),
                    EColaboradorTipo.Colaborador,
                    EColaboradorVinculo.Estagio
                )
            );
            Assert.Equal("CPF_DIGITOS", ex.Message);
        }

        [Fact]
        public void CriarColaborador_ComDataAdmissaoFutura_DeveLancarExcecao()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Colaborador.Criar(
                    "Carlos",
                    "12345678900",
                    GetValidNascimento(),
                    "11999999999",
                    "carlos@email.com",
                    GetValidLogradouro(),
                    "100",
                    "Sala 1",
                    "Senha@123",
                    GetValidArquivo(),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)), // futura
                    EColaboradorTipo.Colaborador,
                    EColaboradorVinculo.Estagio
                )
            );
            Assert.Equal("DATA_ADMISSAO_MAIOR_ATUAL", ex.Message);
        }

        [Fact]
        public void CriarColaborador_ComTipoInvalido_DeveLancarExcecao()
        {
            var tipoInvalido = (EColaboradorTipo)999;

            var ex = Assert.Throws<DomainException>(() =>
                Colaborador.Criar(
                    "Joana",
                    "12345678900",
                    GetValidNascimento(),
                    "11999999999",
                    "joana@email.com",
                    GetValidLogradouro(),
                    "100",
                    "Sala 1",
                    "Senha@123",
                    GetValidArquivo(),
                    GetValidAdmissao(),
                    tipoInvalido,
                    EColaboradorVinculo.Estagio
                )
            );
            Assert.Equal("TIPO_COLABORADOR_INVALIDO", ex.Message);
        }

        [Fact]
        public void CriarColaborador_ComVinculoInvalido_DeveLancarExcecao()
        {
            var vinculoInvalido = (EColaboradorVinculo)999;

            var ex = Assert.Throws<DomainException>(() =>
                Colaborador.Criar(
                    "José",
                    "12345678900",
                    GetValidNascimento(),
                    "11999999999",
                    "jose@email.com",
                    GetValidLogradouro(),
                    "100",
                    "Sala 1",
                    "Senha@123",
                    GetValidArquivo(),
                    GetValidAdmissao(),
                    EColaboradorTipo.Colaborador,
                    vinculoInvalido
                )
            );
            Assert.Equal("VINCULO_COLABORADOR_INVALIDO", ex.Message);
        }

        [Fact]
        public void CriarColaborador_ComTipoColaboradorEClt_DeveLancarExcecao()
        {
            var ex = Assert.Throws<DomainException>(() =>
                Colaborador.Criar(
                    "Beatriz",
                    "12345678900",
                    GetValidNascimento(),
                    "11999999999",
                    "bia@email.com",
                    GetValidLogradouro(),
                    "100",
                    "Sala 1",
                    "Senha@123",
                    GetValidArquivo(),
                    GetValidAdmissao(),
                    EColaboradorTipo.Colaborador,
                    EColaboradorVinculo.CLT
                )
            );
            Assert.Equal("ADMINISTRADOR_CLT_INVALIDO", ex.Message);
        }
    }
}