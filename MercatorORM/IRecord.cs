using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercatorORM
{
    /// <summary>
    /// Cette interface fournit les méthodes de bases pour travailler sur des entrées de base de données façon CRUD
    /// </summary>
    interface IRecord
    {
        /// <summary>
        /// Méthode de création d'entité
        /// </summary>
        /// <returns>true si la création a réussi, false sinon</returns>
        bool Create();

        /// <summary>
        /// Lecture d'une entité
        /// </summary>
        /// <param name="ID">ID de l'entité</param>
        /// <returns>true si la lecture a réussi, false sinon</returns>
        bool Read(object ID, string IDField = null);

        /// <summary>
        /// Mise à jour d'une entité
        /// </summary>
        /// <returns>true si la mise à jour a réussi, false sinon</returns>
        bool Update();

        /// <summary>
        /// Suppression d'une entité
        /// </summary>
        /// <returns>true si la suppression a réussi, false sinon</returns>
        bool Delete();
    }
}
