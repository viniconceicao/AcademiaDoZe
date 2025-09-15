﻿// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace AcademiaDoZe.Application.Tests
{
    public class ColaboradorApplicationTests
    {
        // Configurações de conexão
        const string connectionString = "Server=localhost;Port=3306;Database=db_academia_do_ze;User Id=root;Password=admin;";
        const EAppDatabaseType databaseType = EAppDatabaseType.MySql;
        [Fact(Timeout = 60000)]
        public async Task ColaboradorService_Integracao_Adicionar_Obter_Atualizar_Remover()
        {
            // Arrange: DI usando repositório real (Infra)
            // Configuração dos serviços usando a classe DependencyInjection
            var services = DependencyInjection.ConfigureServices(connectionString, databaseType);
            // Cria o provedor de serviços
            var provider = DependencyInjection.BuildServiceProvider(services);
            // Obtém os serviços necessários via injeção de dependência
            var colaboradorService = provider.GetRequiredService<IColaboradorService>();
            var logradouroService = provider.GetRequiredService<ILogradouroService>();
            // Gera um CPF único para evitar conflito
            var _cpf = GerarCpfFake();
            // obter o logradouro
            var logradouro = await logradouroService.ObterPorIdAsync(5);
            Assert.NotNull(logradouro);
            Assert.Equal(5, logradouro!.Id);
            // cria um arquivo (para facilitar, copie uma foto para dentro do diretório com os fontes do teste)
            // caminho relativo da foto
            var caminhoFoto = Path.Combine("..", "..", "..", "foto_teste.png");
            ArquivoDTO foto = new();

            if (File.Exists(caminhoFoto))
            {
                foto.Conteudo = await File.ReadAllBytesAsync(caminhoFoto);
            }
            else
            {
                foto.Conteudo = null;
                Assert.True(false, "Foto de teste não encontrada.");
            }
            var dto = new ColaboradorDTO
            {
                Nome = "Colaborador Teste",
                Cpf = _cpf,
                DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                Telefone = "11999999999",
                Email = "Colaborador@teste.com",
                Endereco = logradouro,
                Numero = "100",
                Complemento = "Apto 1",
                Senha = "Senha@1",
                Foto = foto,
                DataAdmissao = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
                Tipo = EAppColaboradorTipo.Instrutor,
                Vinculo = EAppColaboradorVinculo.CLT
            };
            ColaboradorDTO? criado = null;
            try
            {
                // Act - Adicionar
                criado = await colaboradorService.AdicionarAsync(dto);
                // Assert - criação
                Assert.NotNull(criado);
                Assert.True(criado!.Id > 0);
                // Act - Obter por cpf
                var obtido = await colaboradorService.ObterPorCpfAsync(criado.Cpf);
                // Assert - obter
                Assert.NotNull(obtido);
                Assert.Equal(criado.Id, obtido!.Id);
                Assert.Equal(_cpf, obtido.Cpf);
                // Act - Atualizar
                var atualizar = new ColaboradorDTO
                {
                    Id = criado.Id,
                    Nome = "Colaborador Atualizado",
                    Cpf = criado.Cpf,
                    DataNascimento = criado.DataNascimento,
                    Telefone = criado.Telefone,
                    Email = criado.Email,
                    Endereco = criado.Endereco,
                    Numero = criado.Numero,
                    Complemento = criado.Complemento,
                    Senha = criado.Senha,
                    Foto = criado.Foto,
                    DataAdmissao = criado.DataAdmissao,
                    Tipo = criado.Tipo,
                    Vinculo = criado.Vinculo
                };
                var atualizado = await colaboradorService.AtualizarAsync(atualizar);
                // Assert - atualizar
                Assert.NotNull(atualizado);
                Assert.Equal("Colaborador Atualizado", atualizado.Nome);
                // Act - Remover
                var removido = await colaboradorService.RemoverAsync(criado.Id);
                Assert.True(removido);
                // Act - Conferir remoção
                var aposRemocao = await colaboradorService.ObterPorIdAsync(criado.Id);
                Assert.Null(aposRemocao);
            }
            finally
            {
                // Clean-up defensivo (se algo falhar antes do remove)
                if (criado is not null)
                {
                    try { await colaboradorService.RemoverAsync(criado.Id); } catch { }
                }
            }
        }
        // Helper simples para gerar um CPF numérico de 11 dígitos (sem validação de dígito verificador)
        private static string GerarCpfFake()
        {
            var rnd = new Random();
            return string.Concat(Enumerable.Range(0, 11).Select(_ => rnd.Next(0, 10).ToString()));
        }
    }
}