// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace AcademiaDoZe.Application.Tests
{
    public class LogradouroApplicationTests
    {
        // Configurações de conexão
        const string connectionString = "Server=localhost;Port=3306;Database=db_academia_do_ze;User Id=root;Password=admin;";
        const EAppDatabaseType databaseType = EAppDatabaseType.MySql;

        private string GerarCepUnico()
        {
            // Gera um CEP numérico com exatamente 8 dígitos
            return Random.Shared.Next(10_000_000, 99_999_999).ToString();
        }

        [Fact(Timeout = 60000)]
        public async Task LogradouroService_Integracao_Adicionar_Obter_Atualizar_Remover()
        {
            // Arrange: DI usando repositório real (Infra)
            var services = DependencyInjection.ConfigureServices(connectionString, databaseType);
            var provider = DependencyInjection.BuildServiceProvider(services);
            var logradouroService = provider.GetRequiredService<ILogradouroService>();

            // CEP único para o teste
            var _cep = GerarCepUnico();

            var dto = new LogradouroDTO
            {
                Cep = _cep,
                Nome = "Rua Teste",
                Bairro = "Centro",
                Cidade = "Cidade X",
                Estado = "SP",
                Pais = "Brasil"
            };
            
            LogradouroDTO? criado = null;
            try
            {
                // Limpeza preventiva
                var existente = await logradouroService.ObterPorCepAsync(_cep);
                if (existente != null)
                {
                    await logradouroService.RemoverAsync(existente.Id);
                }

                // Act - Adicionar
                criado = await logradouroService.AdicionarAsync(dto);
                Assert.NotNull(criado);
                Assert.True(criado!.Id > 0);
                
                // Act - Obter por CEP
                var obtido = await logradouroService.ObterPorCepAsync(_cep);
                Assert.NotNull(obtido);
                Assert.Equal("Rua Teste", obtido!.Nome);

                // Act - Atualizar
                var atualizarDto = new LogradouroDTO
                {
                    Id = criado.Id,
                    Cep = criado.Cep,
                    Nome = "Rua Atualizada",
                    Bairro = criado.Bairro,
                    Cidade = criado.Cidade,
                    Estado = "RJ",
                    Pais = criado.Pais
                };
                var atualizado = await logradouroService.AtualizarAsync(atualizarDto);

                Assert.NotNull(atualizado);
                Assert.Equal("Rua Atualizada", atualizado.Nome);
                Assert.Equal("RJ", atualizado.Estado);

                // Act - Remover
                var removido = await logradouroService.RemoverAsync(criado.Id);
                Assert.True(removido);
                
                // Assert - Conferir remoção
                var aposRemocao = await logradouroService.ObterPorIdAsync(criado.Id);
                Assert.Null(aposRemocao);
            }
            finally
            {
                // Clean-up defensivo (se algo falhar antes do remove)
                if (criado is not null)
                {
                    try { await logradouroService.RemoverAsync(criado.Id); } catch { }
                }
            }
        }
    }
}