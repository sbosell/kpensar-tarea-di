using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace Tarea.Modelos
{
    [TableName("Tarea")]
    [PrimaryKey("Id")]
    public class Tarea
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Desc { get; set; }
        public string Status { get; set; }
    }
}
