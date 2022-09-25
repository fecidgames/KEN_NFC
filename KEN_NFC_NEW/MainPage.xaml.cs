using Plugin.NFC;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Extensions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace KEN_NFC_NEW {
    public partial class MainPage : ContentPage, INotifyPropertyChanged {
		public const string ALERT_TITLE = "NFC";
		public const string MIME_TYPE = "application/com.ken.nfcapp";

        NFCNdefTypeFormat _type;
		bool _makeReadOnly = false;
		bool _eventsAlreadySubscribed = false;
		bool _isDeviceiOS = false;

        public bool DeviceIsListening {
			get => _deviceIsListening;
            set {
				_deviceIsListening = value;
				OnPropertyChanged(nameof(DeviceIsListening));
			}
		}
		private bool _deviceIsListening;
		private bool _nfcIsEnabled;

        public bool NfcIsEnabled {
			get => _nfcIsEnabled;
            set {
				_nfcIsEnabled = value;
				OnPropertyChanged(nameof(NfcIsEnabled));
				OnPropertyChanged(nameof(NfcIsDisabled));
			}
		}

		public bool NfcIsDisabled => !NfcIsEnabled;
		private bool replacePopupGone = false;

        public MainPage() {
			InitializeComponent();
			if(!Transporter.replaceMode) {
                if(Transporter.code != null && Value_Entry != null) {
					Value_Entry.Text = Transporter.code;
					Transporter.code = "";
				}
            } else {
                if(Transporter.code != null) {
					Value_Entry.Text = Transporter.code;
					Transporter.code = "";
				}

				ShowAlert("Scan de NFC-tag om de nieuwe waarde te schrijven.");
				replacePopupGone = true;
			}
        }

		protected async override void OnAppearing() {
			base.OnAppearing();

			CrossNFC.Legacy = false;

			if(CrossNFC.IsSupported) {
				if(!CrossNFC.Current.IsAvailable)
					await ShowAlert("NFC is not available");

				NfcIsEnabled = CrossNFC.Current.IsEnabled;
				Console.WriteLine("NFC status: " + NfcIsEnabled);
				if(!NfcIsEnabled)
					await ShowAlert("NFC is disabled");

				Console.WriteLine(NfcIsDisabled);

				if(Device.RuntimePlatform == Device.iOS)
					_isDeviceiOS = true;

				SubscribeEvents();

				await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
				await StartListeningIfNotiOS();

				if(Transporter.replaceMode && replacePopupGone)
					await Publish(NFCNdefTypeFormat.Uri);
			}
		}

        protected override bool OnBackButtonPressed() {
			UnsubscribeEvents();
			CrossNFC.Current.StopListening();
			Transporter.replaceMode = false;

			return base.OnBackButtonPressed();
		}

		void Button_Clicked_Scan(object sender, EventArgs e) {
			App.Current.MainPage = new NavigationPage(new ScannerPage());
			Transporter.replaceMode = false;
		}
		async void Button_Clicked_StartWriting(object sender, System.EventArgs e) {
			if(Value_Entry.Text != null && Value_Entry.Text != "") {
				var locPerm = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
				if(locPerm != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
					await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);

				await Publish(NFCNdefTypeFormat.Uri);
			} else
				Acr.UserDialogs.Extended.UserDialogs.Instance.Toast("De waarde kan niet leeg zijn.", new TimeSpan(3));

		}

		async void Button_Clicked_Replace(object sender, System.EventArgs e) {
			Transporter.replaceMode = true;
			await Navigation.PushPopupAsync(new ReplacePopup());
		}


		void SubscribeEvents() {
			if(_eventsAlreadySubscribed)
				return;

			_eventsAlreadySubscribed = true;

			CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
			CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
			CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
			CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
			CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;

			if(_isDeviceiOS)
				CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
		}

		void UnsubscribeEvents() {
			Console.WriteLine("Unsubscribing...");
			CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
			CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
			CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
			CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
			CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;

			if(_isDeviceiOS)
				CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;
		}

		/// Event raised when Listener Status has changed
		void Current_OnTagListeningStatusChanged(bool isListening) => DeviceIsListening = isListening;

		/// Event raised when NFC Status has changed
		async void Current_OnNfcStatusChanged(bool isEnabled) {
			NfcIsEnabled = isEnabled;
			await ShowAlert($"NFC has been {(isEnabled ? "enabled" : "disabled")}");
		}

		/// Event raised when an NDEF message is received
		async void Current_OnMessageReceived(ITagInfo tagInfo) {
			App.Current.MainPage = new NavigationPage(new MainPage());

			if(tagInfo == null) {
				await ShowAlert("No tag found");
				return;
			}

			// Customized serial number
			var identifier = tagInfo.Identifier;
			var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
			var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"Tag [{serialNumber}]" : "Tag Info";

			if(!tagInfo.IsSupported)
				await ShowAlert("Onherkenbare tag. Deze tag wordt niet ondersteund door dit apparaat of werkt incorrect.", title);
			else if(tagInfo.IsEmpty)
				await ShowAlert("Empty tag", title);
			else {
				var first = tagInfo.Records[0];
				await ShowAlert(GetMessage(first) + "\nThe text has been saved to a file.", title);

				//Saving text to a txt file here (override possible old file):
				bool restart = false;
				try {
					var filePerm = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
					if(filePerm != Xamarin.Essentials.PermissionStatus.Granted) {
						filePerm = await Permissions.RequestAsync<Permissions.StorageWrite>();
						restart = true;
					}

					DependencyService.Get<IFileService>().SaveTextFile("ken-nfcresult.txt", FileOutput(tagInfo.Records[0].Message));
					Acr.UserDialogs.Extended.UserDialogs.Instance.Toast("Opgeslagen!", new TimeSpan(3));

					if(restart)
						App.Current.MainPage = new NavigationPage(new MainPage());
				} catch(Exception e) {
					Acr.UserDialogs.Extended.UserDialogs.Instance.Toast("Opslaan mislukt, zie console.", new TimeSpan(3));
					Console.WriteLine("Stacktrace: " + e.Message);
				}
			}
		}

		//Formats the tag link (msg) to put in the file
		string FileOutput(string msg) {
			string id = msg.Split('=')[1];
			string oldid = "O:" + ((Transporter.replaceMode) ? Transporter.oldCode : "null");
			string datetime = DateTime.Now.ToString("dd-MM-yyyy;HH:mm:ss");
			string loc = Geolocation.GetLastKnownLocationAsync().Result.ToString();
			string link = msg;

			Transporter.replaceMode = false;
			Transporter.code = "";
			Value_Entry.Text = "";

			return id + ";" + oldid + ";" + datetime + ";" + loc + ";" + link;
		}

		/// Event raised when user cancelled NFC session on iOS 
		void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) => Debug("iOS NFC Session has been cancelled");

		/// Event raised when data has been published on the tag
		async void Current_OnMessagePublished(ITagInfo tagInfo) {
			try {
				CrossNFC.Current.StopPublishing();
				if(tagInfo.IsEmpty)
					await ShowAlert("Chip is gereset.");
				else {
					try {
						var filePerm = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
						if(filePerm != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
							await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);

						DependencyService.Get<IFileService>().SaveTextFile("ken-nfcresult.txt", FileOutput(tagInfo.Records[0].Message));

						Acr.UserDialogs.Extended.UserDialogs.Instance.Toast("De waarde is op de chip geplaatst en opgeslagen", new TimeSpan(3));
						Transporter.replaceMode = false;
					} catch(Exception ex) {
						Acr.UserDialogs.Extended.UserDialogs.Instance.Toast("De waarde is op de chip geplaatst, maar het bestand is niet geschreven. Zie console.", new TimeSpan(3));
						Console.WriteLine("StackTrace: " + ex.Message);
					}
				}

			} catch(Exception ex) {
				await ShowAlert(ex.Message);
			}
		}

		/// Event raised when a NFC Tag is discovered
		async void Current_OnTagDiscovered(ITagInfo tagInfo, bool format) {
			App.Current.MainPage = new NavigationPage(new MainPage());

			if(!CrossNFC.Current.IsWritingTagSupported) {
				await ShowAlert("Writing tag is not supported on this device");
				return;
			}

			try {
				NFCNdefRecord record = null;
				switch(_type) {
					case NFCNdefTypeFormat.WellKnown:
						record = new NFCNdefRecord {
							TypeFormat = NFCNdefTypeFormat.WellKnown,
							MimeType = MIME_TYPE,
							Payload = (Transporter.replaceMode) ? NFCUtils.EncodeToByteArray(Transporter.code) : NFCUtils.EncodeToByteArray(Value_Entry.Text),
							LanguageCode = "en"
						};
						break;
					case NFCNdefTypeFormat.Uri:
						record = new NFCNdefRecord {
							TypeFormat = NFCNdefTypeFormat.Uri,
							Payload = NFCUtils.EncodeToByteArray("http://nfc.ken-monitoring.nl/tag.php?tag=" + Value_Entry.Text)
						};
						break;
					case NFCNdefTypeFormat.Mime:
						record = new NFCNdefRecord {
							TypeFormat = NFCNdefTypeFormat.Mime,
							MimeType = MIME_TYPE,
							Payload = NFCUtils.EncodeToByteArray("Plugin.NFC is awesome!")
						};
						break;
					default:
						break;
				}

				if(!format && record == null)
					throw new Exception("Record can't be null.");

				tagInfo.Records = new[] { record };

				if(format)
					CrossNFC.Current.ClearMessage(tagInfo);
				else
					CrossNFC.Current.PublishMessage(tagInfo, _makeReadOnly);
			} catch(Exception ex) {
				await ShowAlert(ex.Message);
			}
		}

		/// Task to publish data to the tag
		async Task Publish(NFCNdefTypeFormat? type = null) {
			await StartListeningIfNotiOS();
			try {
				_type = NFCNdefTypeFormat.Empty;
				_makeReadOnly = false;

				if(type.HasValue)
					_type = type.Value;
				CrossNFC.Current.StartPublishing(!type.HasValue);
			} catch(Exception ex) {
				await ShowAlert(ex.Message);
			}
		}

		///  Task to publish data to the tag while listening for a tag
		async Task PublishWhileListening(NFCNdefTypeFormat? type = null) {
			try {
				_type = NFCNdefTypeFormat.Empty;
				_makeReadOnly = false;

				if(type.HasValue)
					_type = type.Value;
				CrossNFC.Current.StartPublishing(!type.HasValue);
			} catch(Exception ex) {
				await ShowAlert(ex.Message);
			}
		}

		/// Returns the tag information from NDEF record
		string GetMessage(NFCNdefRecord record) {
			var message = $"Message: {record.Message}";
			message += Environment.NewLine;
			message += $"RawMessage: {Encoding.UTF8.GetString(record.Payload)}";
			message += Environment.NewLine;
			message += $"Type: {record.TypeFormat}";

			if(!string.IsNullOrWhiteSpace(record.MimeType)) {
				message += Environment.NewLine;
				message += $"MimeType: {record.MimeType}";
			}

			return message;
		}

		/// Write a debug message in the debug console
		void Debug(string message) => System.Diagnostics.Debug.WriteLine(message);

		/// Display an alert
		Task ShowAlert(string message, string title = null) => DisplayAlert(string.IsNullOrWhiteSpace(title) ? ALERT_TITLE : title, message, "OK");

		/// Task to start listening for NFC tags if the user's device platform is not iOS
		async Task StartListeningIfNotiOS() {
			if(_isDeviceiOS)
				return;
			await BeginListening();
		}

		/// Task to safely start listening for NFC Tags
		async Task BeginListening() {
			try {
				CrossNFC.Current.StartListening();
			} catch(Exception ex) {
				Console.WriteLine("BeginListening error, restarting page...");
				App.Current.MainPage = new NavigationPage(new MainPage());
			}
		}

		/// Task to safely stop listening for NFC tags
		async Task StopListening() {
			try {
				CrossNFC.Current.StopListening();
			} catch(Exception ex) {
				Console.WriteLine("StopListening error");
				await ShowAlert(ex.Message);
			}
		}
    }
}