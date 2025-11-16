using wmine.Models;

namespace wmine.Core.Services
{
    /// <summary>
    /// Service de gestion centralisée de l'état de l'application
    /// </summary>
    public class ApplicationState
    {
        private static ApplicationState? _instance;
        private static readonly object _lock = new object();

        public static ApplicationState Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ApplicationState();
                    }
                }
                return _instance;
            }
        }

        private Filon? _currentFilon;
        private MineralType? _currentFilter;
        private bool _isAddPinMode;
        private bool _isDirty;

        public Filon? CurrentFilon
        {
            get => _currentFilon;
            set
            {
                if (_currentFilon != value)
                {
                    _currentFilon = value;
                    OnStateChanged();
                }
            }
        }

        public MineralType? CurrentFilter
        {
            get => _currentFilter;
            set
            {
                if (_currentFilter != value)
                {
                    _currentFilter = value;
                    OnStateChanged();
                }
            }
        }

        public bool IsAddPinMode
        {
            get => _isAddPinMode;
            set
            {
                if (_isAddPinMode != value)
                {
                    _isAddPinMode = value;
                    OnStateChanged();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnStateChanged();
                }
            }
        }

        public event EventHandler? StateChanged;

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            _currentFilon = null;
            _currentFilter = null;
            _isAddPinMode = false;
            _isDirty = false;
            OnStateChanged();
        }
    }
}
