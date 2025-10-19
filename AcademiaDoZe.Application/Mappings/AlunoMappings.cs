﻿// Aluno: Vinicius de Liz da Conceição
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
                    ContentType = ".jpg"  // Adiciona o ContentType padrão
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
        /*
        * A camada de aplicação não expõe a senha do Aluno na DTO, ao tentar usar essas entidades, por exemplo, na matricula, pode ocorrer erro de validação/normalização do domínio, quando a DTO for mapeada para a entidade.
        * Ou seja, ao tentar salvar uma nova matricula, a DTO do Aluno vai ser mapeada para a entidade Aluno para poder ser enviada a camada de Infraestrutura, porém, como na DTO a senha do Aluno foi definida como null, ao
        realizar o mapeamento, a validação de domínio da entidade falha, pois a senha é uma campo obrigatório.
        * A prática mais robusta para resolver isso, é criar um DTO ou Mapeamento específico para o caso de uso.
        * O mapeamento ToEntityMatricula() será utilizado exclusivamente na Matricula, e mascara a senha, passando desta forma pela validação.
        */
        public static Aluno ToEntityMatricula(this AlunoDTO alunoDto)
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
                "senhaFalsaSomenteParaAtenderDominio@123",
                (alunoDto.Foto?.Conteudo != null && alunoDto.Foto.ContentType != null)
                    ? Arquivo.Criar(alunoDto.Foto.Conteudo, alunoDto.Foto.ContentType)
                    : null!
            );
        }
        public static Aluno UpdateFromDto(this Aluno aluno, AlunoDTO alunoDto)
        {
            // Se tiver uma nova foto, usa ela, senão mantém a atual
            var novaFoto = alunoDto.Foto?.Conteudo != null
                ? Arquivo.Criar(alunoDto.Foto.Conteudo, alunoDto.Foto.ContentType ?? ".jpg")
                : aluno.Foto;

            var updated = Aluno.Criar(
                alunoDto.Nome ?? aluno.Nome,
                aluno.Cpf, // CPF não pode ser alterado
                alunoDto.DataNascimento != default ? alunoDto.DataNascimento : aluno.DataNascimento,
                alunoDto.Telefone ?? aluno.Telefone,
                alunoDto.Email ?? aluno.Email,
                alunoDto.Endereco.ToEntity() ?? aluno.Endereco,
                alunoDto.Numero ?? aluno.Numero,
                alunoDto.Complemento ?? aluno.Complemento,
                alunoDto.Senha ?? aluno.Senha,
                novaFoto
            );

            // Mantém o ID original usando reflection
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(updated, aluno.Id);

            return updated;
        }
    }
}