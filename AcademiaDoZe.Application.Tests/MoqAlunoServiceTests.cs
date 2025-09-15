// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Moq;
using Xunit;

namespace AcademiaDoZe.Tests.MoqServices
{
    public class MoqAlunoServiceTests
    {
        [Fact]
        public async Task Deve_Verificar_Se_Cpf_Ja_Existe()
        {
            var aluno = new AlunoDTO
            {
                Id = 1,
                Nome = "Maria Souza",
                Cpf = "98765432100",
                DataNascimento = new DateOnly(1995, 5, 15),
                Telefone = "48988888888",
                Endereco = new LogradouroDTO
                {
                    Id = 1,
                    Cep = "88500001",
                    Nome = "Rua B",
                    Bairro = "Coral",
                    Cidade = "Lages",
                    Estado = "SC",
                    Pais = "Brasil"
                },
                Numero = "200"
            };

            var mock = new Mock<IAlunoService>();
            mock.Setup(s => s.CpfJaExisteAsync(aluno.Cpf, null)).ReturnsAsync(true);

            var resultado = await mock.Object.CpfJaExisteAsync(aluno.Cpf);

            Assert.True(resultado);
        }
    }
}
