// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using Moq;
using Xunit;

namespace AcademiaDoZe.Tests.Application
{
    public class MatriculaApplicationTests
    {
        private readonly Mock<IMatriculaService> _matriculaServiceMock;

        public MatriculaApplicationTests()
        {
            _matriculaServiceMock = new Mock<IMatriculaService>();
        }

        [Fact]
        public async Task Deve_Adicionar_Matricula_Com_Sucesso()
        {
            var aluno = new AlunoDTO
            {
                Id = 1,
                Nome = "Carlos Pereira",
                Cpf = "11122233344",
                DataNascimento = new DateOnly(1998, 3, 10),
                Telefone = "48977777777",
                Endereco = new LogradouroDTO
                {
                    Id = 1,
                    Cep = "88500002",
                    Nome = "Rua C",
                    Bairro = "Universitário",
                    Cidade = "Lages",
                    Estado = "SC",
                    Pais = "Brasil"
                },
                Numero = "300"
            };

            var matricula = new MatriculaDTO
            {
                Id = 1,
                AlunoMatricula = aluno,
                Plano = EAppMatriculaPlano.Mensal,
                DataInicio = new DateOnly(2025, 1, 1),
                DataFim = new DateOnly(2025, 1, 31),
                Objetivo = "Ganhar massa muscular",
                RestricoesMedicas = EAppMatriculaRestricoes.None
            };

            _matriculaServiceMock
                .Setup(x => x.AdicionarAsync(It.IsAny<MatriculaDTO>()))
                .ReturnsAsync(matricula);

            var resultado = await _matriculaServiceMock.Object.AdicionarAsync(matricula);

            Assert.NotNull(resultado);
            Assert.Equal(EAppMatriculaPlano.Mensal, resultado.Plano);
        }
    }
}
