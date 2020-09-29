using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FirewallRules
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    /*  TO DO!!!
     *  1. Prouči još asinkroni kod, ovo se može još poboljšati
     *  2. Umjesto fiksno jednog rule-a, možda bi bilo dobro prikazati korisniku listu svih rule-ova te da korisnik sam odabire
     *          koji rule želi uključiti/isključiti... Ako promijeniš naziv rule-a, sve pada u vodu... :(
     *  3. Uljepšaj animacije i hendlaj UI i pozicioniranje elemenata kad se prozor proširuje/smanjuje
     *  4. Hendlaj prava, jer da bi aplikacija radila, mora se pokrenuti kao admin (ili gotova aplikacija ili visual studio). 
     *          Microsoft ovo sigurno ima pokriveno. Istraži
     *  5. Možda dodati login/logout logiku?
     *  6. SetTimer/Countdown logika? Da se određeni rule uključi/isključi u specifično vrijeme ili za x sekundi/minuta/sati...
     *  7. Ikona aplikacije...
     */
    public partial class MainWindow : Window
    {
        private bool animacijaGotova = true;
        private bool taskInProggress = false;
        private bool connectionDisabled = true;
        private bool ruleExists = false;
        private FirewallRule fw = new FirewallRule();
        int animationDurationInMiliSeconds = 500;

        const string RULE_DOES_NOT_EXIST = "Rule doesn't exist. Check firewall rules.";
        const string RULE_STATUS_ENABLED = "Enabled";
        const string RULE_STATUS_DISABLED = "Disabled";
        const string RULE_STATUS_ENABLE = "Enable?";
        const string RULE_STATUS_DISABLE = "Disable?";
        const string ENABLE_STATUS = "Enabled!";
        const string DISABLE_STATUS = "Disabled!";
        const string IMAGE_PATH_DISABLE_HOVER = @"\Resources\disable_hover.png";
        const string IMAGE_PATH_ENABLE_HOVER = @"\Resources\enable_hover.png";
        const string IMAGE_PATH_ENABLE_DEFAULT = @"\Resources\enable_default.png";
        const string IMAGE_PATH_DISABLE_DEFAULT = @"\Resources\disable_default.png";
        const string IMAGE_PATH_GREEN_SPHERE = @"\Resources\green_sphere.png";
        const string IMAGE_PATH_RED_SPHERE = @"\Resources\red_sphere.png";

        public MainWindow()
        {
            InitializeComponent();
            ruleExists = fw.DoesRuleExists();
            statusRuleChangeAlertText.Visibility = Visibility.Hidden;

            if (ruleExists)
            {
                fw.IISAndroidTestChangeRuleStatus(true);
                ruleDoesNotExistText.Visibility = Visibility.Hidden;
                SetImage(IMAGE_PATH_DISABLE_HOVER);

                if (connectionDisabled)
                {
                    statusRuleTextValue.Content = RULE_STATUS_DISABLED;
                    statusRuleTextValue.Foreground = Brushes.PaleVioletRed;
                }
                else
                {
                    statusRuleTextValue.Content = RULE_STATUS_ENABLED;
                    statusRuleTextValue.Foreground = Brushes.LimeGreen;
                }
            }
            else
            {
                ruleDoesNotExistText.Content = RULE_DOES_NOT_EXIST;
                ruleDoesNotExistText.Visibility = Visibility.Visible;
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (connectionDisabled)
            {
                SetImage(IMAGE_PATH_ENABLE_HOVER);

                if (animacijaGotova || !taskInProggress)
                {
                    statusRuleChangeAlertText.Content = RULE_STATUS_ENABLE;
                    statusRuleChangeAlertText.Foreground = Brushes.LimeGreen;
                }
            }
            else
            {
                SetImage(IMAGE_PATH_DISABLE_HOVER);

                if (animacijaGotova || !taskInProggress)
                {
                    statusRuleChangeAlertText.Content = RULE_STATUS_DISABLE;
                    statusRuleChangeAlertText.Foreground = Brushes.PaleVioletRed;
                }
            }

            statusRuleChangeAlertText.Visibility = Visibility.Visible;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            if (connectionDisabled)
                SetImage(IMAGE_PATH_DISABLE_HOVER);
            else
                SetImage(IMAGE_PATH_ENABLE_HOVER);

            if (animacijaGotova || !taskInProggress)
                statusRuleChangeAlertText.Visibility = Visibility.Hidden;
        }

        private async void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!taskInProggress)
            {
                connectionDisabled = !connectionDisabled;

                fw.IISAndroidTestChangeRuleStatus(connectionDisabled);
                taskInProggress = true;

                if (connectionDisabled)
                    statusImage.Source = new BitmapImage(new Uri(IMAGE_PATH_RED_SPHERE, UriKind.Relative));
                else
                    statusImage.Source = new BitmapImage(new Uri(IMAGE_PATH_GREEN_SPHERE, UriKind.Relative));

                if (connectionDisabled)
                {
                    SetImage(IMAGE_PATH_DISABLE_DEFAULT);
                    statusRuleTextValue.Content = RULE_STATUS_DISABLED;
                    statusRuleTextValue.Foreground = Brushes.PaleVioletRed;
                    await TextFadeAnimation(animationDurationInMiliSeconds, statusRuleChangeAlertText, false);

                    statusRuleChangeAlertText.Visibility = Visibility.Hidden;
                    statusRuleChangeAlertText.Opacity = 1;
                }
                else
                {
                    SetImage(IMAGE_PATH_ENABLE_DEFAULT);
                    statusRuleTextValue.Content = RULE_STATUS_ENABLED;
                    statusRuleTextValue.Foreground = Brushes.LimeGreen;
                    await TextFadeAnimation(animationDurationInMiliSeconds, statusRuleChangeAlertText, true);

                    statusRuleChangeAlertText.Visibility = Visibility.Hidden;
                    statusRuleChangeAlertText.Opacity = 1;
                }

                await StartImageAnimation(statusImage);
            }
        }

        private void SetImage(string path)
        {
            statusChangeButton.BeginInit();
            statusChangeButton.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            statusChangeButton.EndInit();

        }

        private async Task TextFadeAnimation(int fadeDurationInMiliseconds, Label label, bool enabled)
        {
            animacijaGotova = false;
            if (enabled)
            {
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
                label.Content = ENABLE_STATUS;
                label.Foreground = Brushes.LimeGreen;

                // nađi bolji način za napraviti ovo ispod, jer osobi koja je smislila DRY princip bi otpala kosa
                await OpacityToMax (fadeDurationInMiliseconds, label, enabled);
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMax(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMax(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
            }
            else
            {
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
                label.Content = DISABLE_STATUS;
                label.Foreground = Brushes.PaleVioletRed;

                // nađi bolji način za napraviti ovo ispod, jer osobi koja je smislila DRY princip bi otpala kosa
                await OpacityToMax(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMax(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMax(fadeDurationInMiliseconds, label, enabled);
                await OpacityToMin(fadeDurationInMiliseconds, label, enabled);
            }

            animacijaGotova = true;
            taskInProggress = false;
        }

        private async Task OpacityToMin(int fadeDurationInMiliseconds, Label label, bool enabled)
        {
            double stopaSmanjivanja = 0.15;

            while (label.Opacity > 0.0)
            {
                label.Opacity -= stopaSmanjivanja;
                await Task.Delay(fadeDurationInMiliseconds / 100);
            }
        }

        private async Task OpacityToMax(int fadeDurationInMiliseconds, Label label, bool enabled)
        {
            double stopaSmanjivanja = 0.15;

            while (label.Opacity < 1.0)
            {
                label.Opacity += stopaSmanjivanja;
                await Task.Delay(fadeDurationInMiliseconds / 100);
            }
        }

        private async Task StartImageAnimation(Image sphereImage)
        {
            if (animacijaGotova && !taskInProggress)
            {
                double stopaSmanjivanja = 0.01;
                double predznak = -1.0;
                while (sphereImage.Opacity > 0)
                {
                    sphereImage.Opacity += stopaSmanjivanja * predznak;
                    await Task.Delay(10);

                    if (sphereImage.Opacity < 0.15)
                        predznak = 1.0;
                    else if (sphereImage.Opacity > 0.85)
                        predznak = -1.0;

                    if (!animacijaGotova || taskInProggress)
                    {
                        sphereImage.Opacity = 1;
                        break;
                    }
                }
            }
        }

        private void OnExit(object sender, CancelEventArgs e)
        {
            fw.IISAndroidTestChangeRuleStatus(true);
        }
    }
}
