// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings
{
    public static class ColaboradorMappings
    {
        public static ColaboradorDTO ToDto(this Colaborador colaborador)
        {
            return new ColaboradorDTO
            {
                Id = colaborador.Id, // Mantém o ID original
                Nome = colaborador.Nome,
                Cpf = colaborador.Cpf,
                DataNascimento = colaborador.DataNascimento,
                Telefone = colaborador.Telefone,
                Email = colaborador.Email,
                Endereco = colaborador.Endereco.ToDto(),
                Numero = colaborador.Numero,
                Complemento = colaborador.Complemento,
                Senha = null, // A senha não deve ser exposta no DTO
                Foto = colaborador.Foto != null ? new ArquivoDTO { Conteudo = colaborador.Foto.Conteudo, ContentType = ".jpg" } : null,
                DataAdmissao = colaborador.DataAdmissao,
                Tipo = colaborador.Tipo.ToApp(),
                Vinculo = colaborador.Vinculo.ToApp()
            };
        }

        public static Colaborador ToEntity(this ColaboradorDTO colaboradorDto)
        {
            var colaborador = Colaborador.Criar(
                colaboradorDto.Nome,
                colaboradorDto.Cpf,
                colaboradorDto.DataNascimento,
                colaboradorDto.Telefone,
                colaboradorDto.Email!,
                colaboradorDto.Endereco.ToEntity(),
                colaboradorDto.Numero,
                colaboradorDto.Complemento!,
                colaboradorDto.Senha!,
                colaboradorDto.Foto?.Conteudo != null 
                    ? Arquivo.Criar(
                        colaboradorDto.Foto.Conteudo,
                        colaboradorDto.Foto.ContentType ?? ".jpg"
                      )
                    : null!,
                colaboradorDto.DataAdmissao,
                colaboradorDto.Tipo.ToDomain(),
                colaboradorDto.Vinculo.ToDomain()
            );

            // Define o ID usando reflection
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(colaborador, colaboradorDto.Id);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] ToEntity - ID definido: {colaborador.Id}");
            return colaborador;
        }

        public static Colaborador UpdateFromDto(this Colaborador colaborador, ColaboradorDTO colaboradorDto)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateFromDto - ID Original: {colaborador.Id}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateFromDto - ID do DTO: {colaboradorDto.Id}");

            // Se tiver uma nova foto, usa ela, senão mantém a atual
            var novaFoto = colaboradorDto.Foto?.Conteudo != null
                ? Arquivo.Criar(colaboradorDto.Foto.Conteudo, ".jpg")
                : colaborador.Foto;

            var updated = Colaborador.Criar(
                colaboradorDto.Nome ?? colaborador.Nome,
                colaborador.Cpf, // CPF não pode ser alterado
                colaboradorDto.DataNascimento != default ? colaboradorDto.DataNascimento : colaborador.DataNascimento,
                colaboradorDto.Telefone ?? colaborador.Telefone,
                colaboradorDto.Email ?? colaborador.Email,
                colaboradorDto.Endereco.ToEntity() ?? colaborador.Endereco,
                colaboradorDto.Numero ?? colaborador.Numero,
                colaboradorDto.Complemento ?? colaborador.Complemento,
                colaboradorDto.Senha ?? colaborador.Senha,
                novaFoto,
                colaboradorDto.DataAdmissao != default ? colaboradorDto.DataAdmissao : colaborador.DataAdmissao,
                colaboradorDto.Tipo != default ? colaboradorDto.Tipo.ToDomain() : colaborador.Tipo,
                colaboradorDto.Vinculo != default ? colaboradorDto.Vinculo.ToDomain() : colaborador.Vinculo
            );

            // Define o ID usando reflection - mantém o ID original
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(updated, colaborador.Id);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateFromDto - ID Final: {updated.Id}");
            return updated;
        }
    }
}