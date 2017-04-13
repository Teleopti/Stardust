using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.WinCode.Shifts.Presenters
{
    public abstract class BasePresenter : IPresenterBase
    {
        protected BasePresenter(IExplorerPresenter explorer, IDataHelper dataHelper)
        {
            Explorer = explorer;
            DataWorkHelper = dataHelper;
        }

        public IDataHelper DataWorkHelper { get; private set; }

        public IExplorerPresenter Explorer { get; private set; }

        public virtual bool Validate()
        {
            return true;
        }

	    public virtual void LoadModelCollection()
        {
        }
    }

    public abstract class BasePresenter<T> : BasePresenter, ICommon<T>
    {
        private IList<T> _modelCollection;

        protected BasePresenter(IExplorerPresenter explorer, IDataHelper dataHelper) 
            : base(explorer,dataHelper)
        {
            _modelCollection = new List<T>();
        }

        public ReadOnlyCollection<T> ModelCollection
        {
            get
            {
                return new ReadOnlyCollection<T>(_modelCollection);
            }
        }

        public void SetModelCollection(ReadOnlyCollection<T> models)
        {
            _modelCollection = models.ToList();
        }

        public virtual void AddToModelCollection(T model)
        {
            if(_modelCollection != null)
                _modelCollection.Add(model);
        }

        public void RemoveFromCollection(T model)
        {
            if (_modelCollection != null)
                _modelCollection.Remove(model);
        }

        public void ReplaceModel(T oldOne, T newOne)
        {
            int position = _modelCollection.IndexOf(oldOne);
            _modelCollection.RemoveAt(position);
            _modelCollection.Insert(position, newOne);
        }

        public void SwapModels(T left, T right)
        {
            int leftPos = _modelCollection.IndexOf(left);
            int rightPos = _modelCollection.IndexOf(right);

            _modelCollection.RemoveAt(leftPos);
            _modelCollection.Insert(leftPos, right);

            _modelCollection.RemoveAt(rightPos);
            _modelCollection.Insert(rightPos, left);
        }

        public void ClearModelCollection()
        {
            if (_modelCollection != null)
            {
                _modelCollection.Clear();
            }
        }

        public override bool Validate()
        {
            foreach (T model in _modelCollection)
            {
                var validateModel = model as IValidate;
                if (validateModel==null || !validateModel.Validate())
                    return false;
            }

            return true;
        }
    }
}
