﻿using System;
using DamageMeter.AutoUpdate;
using DamageMeter.UI.Windows;
using Data;
using Nostrum;
using System.Windows.Input;
using System.Windows.Media;
using Lang;
using Tera.Game;

namespace DamageMeter.UI
{
    public class MainViewModel : TSPropertyChanged
    {
        public static MainViewModel Instance { get; private set; }

        private string _windowTitle = "Shinra Meter";
        private bool _paused;
        private bool _waitingMapChangeTBVisible;
        private bool _mapChanged;
        private bool _hideGeneralData;
        private EntityId _hideEid;
        private bool _enableChatAfterOverload = false;


        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (_windowTitle == value) return;
                _windowTitle = value;
                NotifyPropertyChanged();
            }
        }
        public bool Paused
        {
            get => _paused;
            set
            {
                if (_paused == value) return;
                _paused = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(UserPaused));

                if (_paused)
                {
                    _mapChanged = false;
                    WaitingMapChangeTBVisibile = !UserPaused;
                }
                else
                {
                    WaitingMapChangeTBVisibile = !_mapChanged;
                }

            }
        }
        public bool UserPaused => BasicTeraData.Instance.WindowData.UserPaused;
        public bool ShowTimeLeft => BasicTeraData.Instance.WindowData.ShowTimeLeft;
        public double WindowOpacity => BasicTeraData.Instance.WindowData.MainWindowOpacity;
        public bool ShowAdds => PacketProcessor.Instance.TimedEncounter;
        public bool WaitingMapChangeTBVisibile
        {
            get => _waitingMapChangeTBVisible;
            set
            {
                if (_waitingMapChangeTBVisible == value) return;
                _waitingMapChangeTBVisible = value;
                NotifyPropertyChanged();
            }
        }
        public bool HideGeneralData
        {
            get => _hideGeneralData;
            set
            {
                if (_hideGeneralData == value) return;
                _hideGeneralData = value;
                NotifyPropertyChanged();
            }
        }

        private bool _bossGageVisible;
        private string _timerText;
        private string _totalDpsText;
        private string _totalDamageText;

        public bool BossGageVisible
        {
            get => _bossGageVisible;
            set
            {
                if (_bossGageVisible == value) return;
                _bossGageVisible = value;
                NotifyPropertyChanged();
            }
        }


        public string TimerText
        {
            get => _timerText;
            set
            {
                if (_timerText == value) return;
                _timerText = value;
                NotifyPropertyChanged();
            }
        }
        public string TotalDpsText
        {
            get => _totalDpsText;
            set
            {
                if (_totalDpsText == value) return;
                _totalDpsText = value;
                NotifyPropertyChanged();
            }
        }
        public string TotalDamageText
        {
            get => _totalDamageText;
            set
            {
                if (_totalDamageText == value) return;
                _totalDamageText = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand TogglePauseCommand { get; }
        public ICommand ToggleAddsCommand { get; }
        public ICommand SetBossGageVisibilityCommand { get; }
        public ICommand ShowEntityStatsCommand { get; }
        public ICommand ShowUploadHistoryCommand { get; }
        public ICommand ShowBossHPBarCommand { get; }
        public ICommand VerifyCloseCommand { get; }

        private bool _blurPlayerNames;
        private int _queuedPackets;

        public bool BlurPlayerNames
        {
            get => _blurPlayerNames;
            set
            {
                if (_blurPlayerNames == value) return;
                _blurPlayerNames = value;
                NotifyPropertyChanged();
            }
        }



        public MainViewModel()
        {
            Instance = this;

            App.Setup();

            WindowTitle = "Shinra Meter v" + UpdateManager.Version;

            PacketProcessor.Instance.Connected += OnConnected;
            PacketProcessor.Instance.PauseAction += OnPaused;
            PacketProcessor.Instance.MapChangedAction += OnMapChanged;
            PacketProcessor.Instance.DisplayGeneralDataChanged += OnDisplayGeneralDataChanged;
            PacketProcessor.Instance.OverloadedChanged += OnOverloadedChanged;
            PacketProcessor.Instance.TickUpdated += OnUpdate;

            TogglePauseCommand = new RelayCommand(_ => TogglePause());
            ToggleAddsCommand = new RelayCommand(_ => ToggleAdds());
            SetBossGageVisibilityCommand = new RelayCommand(visibility => SetBossGageVisibility((bool.Parse(visibility.ToString()))));
            ShowBossHPBarCommand = new RelayCommand(_ => App.HudContainer.BossGage.ShowWindow());
            ShowEntityStatsCommand = new RelayCommand(_ => App.HudContainer.EntityStats.ShowWindow());
            ShowUploadHistoryCommand = new RelayCommand(_ => App.HudContainer.UploadHistory.ShowWindow());
            VerifyCloseCommand = new RelayCommand(noConfirm => App.VerifyClose((bool.Parse(noConfirm.ToString()))));
        }

        private void OnUpdate(UiUpdateMessage message)
        {
            QueuedPackets = message.QueuedPackets;
            var timeValue = BasicTeraData.Instance.WindowData.ShowTimeLeft && message.StatsSummary.EntityInformation.TimeLeft > 0
                ? message.StatsSummary.EntityInformation.TimeLeft
                : message.StatsSummary.EntityInformation.Interval;

            TimerText = TimeSpan.FromSeconds(timeValue / TimeSpan.TicksPerSecond).ToString(@"mm\:ss");

            TotalDpsText = FormatHelpers.Instance.FormatValue(message.StatsSummary.EntityInformation.Interval == 0
                               ? message.StatsSummary.EntityInformation.TotalDamage
                               : message.StatsSummary.EntityInformation.TotalDamage * TimeSpan.TicksPerSecond / message.StatsSummary.EntityInformation.Interval) + LP.PerSecond;

            TotalDamageText = FormatHelpers.Instance.FormatValue(message.StatsSummary.EntityInformation.TotalDamage);

        }

        public int QueuedPackets
        {
            get => _queuedPackets;
            set
            {
                if (_queuedPackets == value) return;
                _queuedPackets = value;
                NotifyPropertyChanged();
            }
        }


        private void SetBossGageVisibility(bool visibility)
        {
            BossGageVisible = visibility;
        }

        public void TogglePause()
        {
            BasicTeraData.Instance.WindowData.UserPaused = !BasicTeraData.Instance.WindowData.UserPaused;
            if (BasicTeraData.Instance.WindowData.UserPaused)
            {
                PacketProcessor.Instance.NeedPause = true;
            }

            Paused = BasicTeraData.Instance.WindowData.UserPaused;

            SettingsWindowViewModel.NotifyPausedChanged();
        }
        private void ToggleAdds()
        {
            PacketProcessor.Instance.TimedEncounter = !PacketProcessor.Instance.TimedEncounter;
            NotifyPropertyChanged(nameof(ShowAdds));
        }

        private void OnMapChanged()
        {
            if (!Paused)
            {
                _mapChanged = true;
                WaitingMapChangeTBVisibile = false;
            }

            HideGeneralData = false;
            BossGageVisible = true;
        }

        private void OnPaused(bool paused)
        {
            Paused = paused;
        }
        private void OnDisplayGeneralDataChanged(bool hide, EntityId eid)
        {
            if (hide)
            {
                _hideEid = eid;
                HideGeneralData = true;
                SetBossGageVisibility(false);
            }
            else if (_hideEid == eid)
            {
                HideGeneralData = false;
                SetBossGageVisibility(true);
            }
        }
        private void OnConnected(string servername)
        {
            WindowTitle = servername;

        }
        private void OnOverloadedChanged()
        {
            if (PacketProcessor.Instance.Overloaded)
            {
                if (BasicTeraData.Instance.WindowData.EnableChat)
                {
                    BasicTeraData.Instance.WindowData.EnableChat = false;
                    _enableChatAfterOverload = true;
                }
            }
            else
            {
                if (_enableChatAfterOverload)
                    BasicTeraData.Instance.WindowData.EnableChat = true;
            }
        }

    }

}