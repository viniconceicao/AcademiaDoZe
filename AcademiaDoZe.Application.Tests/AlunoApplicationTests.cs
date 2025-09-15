// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Moq;
using Xunit;

namespace AcademiaDoZe.Tests.Application
{
    public class AlunoApplicationTests
    {
        private readonly Mock<IAlunoService> _alunoServiceMock;

        public AlunoApplicationTests()
        {
            _alunoServiceMock = new Mock<IAlunoService>();
        }

        [Fact]
        public async Task Deve_Adicionar_Aluno_Com_Sucesso()
        {
            var aluno = new AlunoDTO
            {
                Id = 1,
                Nome = "João Silva",
                Cpf = "12345678901",
                DataNascimento = new DateOnly(2000, 1, 1),
                Telefone = "48999999999",
                Email = "joao@email.com",
                Endereco = new LogradouroDTO
                {
                    Id = 1,
                    Cep = "88500000",
                    Nome = "Rua A",
                    Bairro = "Centro",
                    Cidade = "Lages",
                    Estado = "SC",
                    Pais = "Brasil"
                },
                Numero = "100"
            };

            _alunoServiceMock
                .Setup(x => x.AdicionarAsync(It.IsAny<AlunoDTO>()))
                .ReturnsAsync(aluno);

            var resultado = await _alunoServiceMock.Object.AdicionarAsync(aluno);

            Assert.NotNull(resultado);
            Assert.Equal("João Silva", resultado.Nome);
        }
    }
}
