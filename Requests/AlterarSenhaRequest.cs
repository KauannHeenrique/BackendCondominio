﻿namespace Condominio_API.Requests
{
    public class AlterarSenhaRequest
    {
        public string SenhaAtual { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
    }
}
