// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings
{
    public static class AlunoMappings
    {
        public static AlunoDTO ToDto(this Aluno aluno)
        {
            return new AlunoDTO
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Cpf = aluno.Cpf,
                DataNascimento = aluno.DataNascimento,
                Telefone = aluno.Telefone,
                Email = aluno.Email,
                Endereco = aluno.Endereco.ToDto(),
                Numero = aluno.Numero,
                Complemento = aluno.Complemento,
                Senha = null, // a senha não deve ser exposta no DTO
                Foto = aluno.Foto != null ? new ArquivoDTO 
                { 
                    Conteudo = aluno.Foto.Conteudo,
                    ContentType = ".jpg"
                } : null
            };
        }
        
        public static Aluno ToEntity(this AlunoDTO alunoDto)
        {
            return Aluno.Criar(
                alunoDto.Nome,
                alunoDto.Cpf,
                alunoDto.DataNascimento,
                alunoDto.Telefone,
                alunoDto.Email!,
                alunoDto.Endereco.ToEntity(),
                alunoDto.Numero,
                alunoDto.Complemento!,
                alunoDto.Senha!,
                alunoDto.Foto?.Conteudo != null 
                    ? Arquivo.Criar(
                        alunoDto.Foto.Conteudo,
                        alunoDto.Foto.ContentType ?? ".jpg"
                      )
                    : null!
            );
        }
        
        public static Aluno ToEntityMatricula(this AlunoDTO alunoDto)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AlunoMappings.ToEntityMatricula - Início");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AlunoMappings.ToEntityMatricula - AlunoDTO.Id: {alunoDto.Id}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AlunoMappings.ToEntityMatricula - AlunoDTO.Nome: {alunoDto.Nome}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AlunoMappings.ToEntityMatricula - AlunoDTO.CPF: {alunoDto.Cpf}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AlunoMappings.ToEntityMatricula - AlunoDTO.Endereco.Id: {alunoDto.Endereco?.Id}");
            
            var aluno = Aluno.Criar(
                alunoDto.Nome,
                alunoDto.Cpf,
                alunoDto.DataNascimento,
                alunoDto.Telefone,
                alunoDto.Email!,
                alunoDto.Endereco.ToEntity(),
                alunoDto.Numero,
                alunoDto.Complemento!,
                "senhaFalsaSomenteParaAtenderDominio@123",
                (alunoDto.Foto?.Conteudo != null && alunoDto.Foto.ContentType != null)
                    ? Arquivo.Criar(alunoDto.Foto.Conteudo, alunoDto.Foto.ContentType)
                    : null!
            );
            
            // IMPORTANTE: Preserva o ID do DTO na entidade
            typeof(Entity).GetProperty("Id")?.SetValue(aluno, alunoDto.Id);
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AlunoMappings.ToEntityMatricula - Aluno.Id após SetValue: {aluno.Id}");
            
            return aluno;
        }
        
        public static Aluno UpdateFromDto(this Aluno aluno, AlunoDTO alunoDto)
        {
            var novaFoto = alunoDto.Foto?.Conteudo != null
                ? Arquivo.Criar(alunoDto.Foto.Conteudo, alunoDto.Foto.ContentType ?? ".jpg")
                : aluno.Foto;

            var updated = Aluno.Criar(
                alunoDto.Nome ?? aluno.Nome,
                aluno.Cpf,
                alunoDto.DataNascimento != default ? alunoDto.DataNascimento : aluno.DataNascimento,
                alunoDto.Telefone ?? aluno.Telefone,
                alunoDto.Email ?? aluno.Email,
                alunoDto.Endereco.ToEntity() ?? aluno.Endereco,
                alunoDto.Numero ?? aluno.Numero,
                alunoDto.Complemento ?? aluno.Complemento,
                alunoDto.Senha ?? aluno.Senha,
                novaFoto
            );

            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(updated, aluno.Id);

            return updated;
        }
    }
}