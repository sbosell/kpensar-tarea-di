using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using FluentValidation;
using FluentValidation.Attributes;

namespace Tarea.Modelos
{
    [TableName("Tarea")]
    [PrimaryKey("Id")]
    [Validator(typeof(TareaValidation))]
    public class Tarea
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Desc { get; set; }
        public string Status { get; set; }
    }

    public class TareaValidation : AbstractValidator<Tarea>
    {
        public TareaValidation()
        {
            RuleFor(t => t.Nombre).NotEmpty().WithMessage("Nombre es obligatorio");
            RuleFor(t => t.Desc).NotEmpty().WithMessage("Descripcion es obligatorio");
            RuleFor(t => t.Status).NotEmpty().WithMessage("Status es obglitatorio");

        }

    }
}
