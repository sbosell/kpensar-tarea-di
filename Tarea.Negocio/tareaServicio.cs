using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tarea.Datos;
using Tarea.Modelos;
using NPoco;

namespace Tarea.Negocio
{
    public interface ITareaServicio
    {
        IEnumerable<Tarea.Modelos.Tarea> Listar();
        Tarea.Modelos.Tarea Obtener(int id);
        Tarea.Modelos.Tarea Eliminar(Tarea.Modelos.Tarea tarea);
        Tarea.Modelos.Tarea Agregar(Tarea.Modelos.Tarea tarea);
    }

    public class TareaServicio : ITareaServicio
    {
        private readonly IRepository<Tarea.Modelos.Tarea> _tareaRepo;
        private readonly IUnitOfWork _uow;

        public TareaServicio(IRepository<Tarea.Modelos.Tarea> tareaRepo, IUnitOfWork uow)
        {
            _tareaRepo = tareaRepo;
            _uow = uow;
           
        }

        public IEnumerable<Tarea.Modelos.Tarea> Listar() {
            return _tareaRepo.Fetch();
        }

        public Tarea.Modelos.Tarea Obtener(int id)
        {
            return _tareaRepo.SingleOrDefault(id);
        }

        public Tarea.Modelos.Tarea Eliminar(Tarea.Modelos.Tarea tarea) {

            _tareaRepo.Delete(tarea);           
           _uow.Commit();

            return tarea;
        }

        public Tarea.Modelos.Tarea Agregar(Tarea.Modelos.Tarea tarea)
        {
            _tareaRepo.Insert(tarea);
            _uow.Commit();

            return tarea;
        }

    }

}
