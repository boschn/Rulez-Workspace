/**
 *  ONTRACK RULEZ DECLARATION
 *  
 * eXpression Tree
 * 
 * Version: 1.0
 * Created: 2015-10-14
 * Last Change
 * 
 * Change Log
 * 
 * (C) by Boris Schneider, 2015
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OnTrack.Core;
using OnTrack.Rulez;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Rulez
{
    /// <summary>
    /// defines a nested repository
    /// </summary>
    public interface IScope : IRepository
    {
        /// <summary>
        /// gets the list of children
        /// </summary>
        ObservableCollection<IScope> SubScopes { get; }
        /// <summary>
        /// gets the Repository of the Scope
        /// </summary>
        IRepository Repository { get; }
        /// <summary>
        /// gets or sets the Engine
        /// </summary>
        Engine Engine { get; set; }
        /// <summary>
        /// returns true if the Children have an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasSubScope(string id);
        /// <summary>
        /// returns true if the scope name exists in the descendants of this scope
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool HasScope(CanonicalName name);
        bool HasScope(string id);
        /// <summary>
        /// returns a Subscope of an given id or null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IScope GetSubScope(string id);
        /// <summary>
        /// returns a scope object from the descendants
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IScope GetScope(CanonicalName name);
        IScope GetScope(string id);
        /// <summary>
        /// create an Subscope of an given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IScope AddSubScope(string id);
        /// <summary>
        /// adds a scope object to the descendants
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool AddScope(IScope scope);
        bool AddScope(string id);
        bool AddScope(CanonicalName name);
        /// <summary>
        /// get root scope
        /// </summary>
        /// <returns></returns>
        IScope GetRoot();
        /// <summary>
        /// creates a new scope object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IScope NewScope(string id);
        /// <summary>
        /// creates a new scope object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IScope NewScope(CanonicalName name);
        /// <summary>
        /// gets the Parent of this Scope
        /// </summary>
        IScope Parent { get; set; }
        /// <summary>
        /// gets or sets the ID of the scope
        /// </summary>
        string Id { get;  }
        /// <summary>
        /// gets or sets the Name of the Scope
        /// </summary>
        CanonicalName Name { get;  }
        

        /// <summary>
        /// event handler for dataObjectRepository Added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Scope_DataObjectRepositoryAdded(object sender, Rulez.Engine.EventArgs e);
    }
    /// <summary>
    /// Interface for Engine Repositories
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// gets the unique handle of the engine
        /// </summary>
        string Id { get; }
        /// <summary>
        /// register the data object repository for this repository
        /// </summary>
        /// <param name="dataObjectRepository"></param>
        /// <returns></returns>
        bool RegisterDataObjectRepository(IDataObjectRepository dataObjectRepository);
        /// <summary>
        /// deregister the data object repository for this repository
        /// </summary>
        /// <param name="dataObjectRepository"></param>
        /// <returns></returns>
        bool DeRegisterDataObjectRepository(IDataObjectRepository dataObjectRepository);
        /// <summary>
        /// adds a ISigned object to the repository
        /// </summary>
        /// <param name="signed"></param>
        /// <returns></returns>
        bool Add(ISigned signed);
        /// <summary>
        /// returns true if the repository has an ISigned object with signature
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        bool Has(ISignature signature);
        /// <summary>
        /// returns true if the ISigned derived type T is in the repository
        /// optional: AND an ISigned of T with the signature exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signature"></param>
        /// <returns></returns>
        bool Has<T>(ISignature signature = null) where T : ISigned;
        /// <summary>
        /// returns true if the repository has an ISigned object with the canonical name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Has(CanonicalName name);
        /// <summary>
        /// returns true if the repository has an ISigned object derived Class T and
        /// the CanonicalName name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Has<T>(CanonicalName name) where T : ISigned;
        /// <summary>
        /// gets all ISigned objects in the repository with the signature or empty list
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        IList<ISigned> Get(ISignature signature);
        /// <summary>
        /// gets all ISigned derived objects with the optional signature
        /// or empty list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signed"></param>
        /// <returns></returns>
        IList<T> Get<T>(ISignature signature = null) where T : ISigned;
        /// <summary>
        /// gets all ISigned derived objects whith the canonical name
        /// or empty list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signed"></param>
        /// <returns></returns>
        IList<T> Get<T>(CanonicalName name) where T : ISigned;
        /// <summary>
        /// remove ISigned object in the repository with the signature
        /// returns true on success
        /// </summary>
        /// <param name="signature"></param>
        /// <returns>True if successfull</returns>
        bool Remove(ISignature signature);
    }
}
