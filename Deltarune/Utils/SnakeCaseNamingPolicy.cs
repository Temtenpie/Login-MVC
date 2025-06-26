using System.Text;
using System.Text.Json;

namespace Deltarune.Utils
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        /// <param name="name">Nombre de la propiedad en PascalCase</param>
        /// <returns>Nombre convertido a snake_case</returns>
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c))
                {
                    // Agrega guión bajo antes de mayúsculas (excepto la primera)
                    if (i > 0)
                        stringBuilder.Append('_');
                    stringBuilder.Append(char.ToLower(c));
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
