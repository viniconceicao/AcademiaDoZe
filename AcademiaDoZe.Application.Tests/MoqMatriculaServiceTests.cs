// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using Moq;
using Xunit;

namespace AcademiaDoZe.Tests.MoqServices
{
    public class MoqMatriculaServiceTests
    {
        [Fact]
        public async Task Deve_Buscar_Matriculas_Ativas()
        {
            var mock = new Mock<IMatriculaService>();
            var matriculas = new List<MatriculaDTO>
            {
                new MatriculaDTO
                {
                    Id = 1,
                    AlunoMatricula = new AlunoDTO
                    {
                        Id = 1,
                        Nome = "Pedro Oliveira",
                        Cpf = "12312312399",
                        DataNascimento = new DateOnly(2002, 7, 20),
                        Telefone = "48966666666",
                        Endereco = new LogradouroDTO
                        {
                            Id = 2,
                            Cep = "88500003",
                            Nome = "Rua D",
                            Bairro = "São Cristóvão",
                            Cidade = "Lages",
                            Estado = "SC",
                            Pais = "Brasil"
                        },
                        Numero = "400"
                    },
                    Plano = EAppMatriculaPlano.Trimestral,
                    DataInicio = new DateOnly(2025, 2, 1),
                    DataFim = new DateOnly(2025, 4, 30),
                    Objetivo = "Perder peso",
                    RestricoesMedicas = EAppMatriculaRestricoes.Diabetes
                }
            };

            mock.Setup(s => s.ObterAtivasAsync(0)).ReturnsAsync(matriculas);

            var resultado = await mock.Object.ObterAtivasAsync();

            Assert.NotEmpty(resultado);
            Assert.Single(resultado);
        }
    }
}
