/*
 * Interfejs IQATestLaunchPoint zawiera specyfikację wymagań QATestLauncher, czyli uniwersalnej czarnej skrzynki, która robi test
 * 
 * Dodatkowo, na własne potrzeby, launcher typu musi mieć:
 * - na przykładzie launchera typu Form
 * -- funkcje aktualizujące GUI
 * -- delegaty
 * -- invoke tych funkcji aktualizujących przez delegaty
 * - w Form1 są to odpowiednio
 * -- private void UpdateProgress(); private void UpdateResult()
 * -- private delegate void UpdateResultDelegate(); private delegate void UpdateProgressDelegate();
 * -- Invoke wewnątrz OnTestFinish(new UpdateResultDelegate(UpdateResult)); Invoke wewnątrz OnTestProgress(new UpdateProgressDelegate(UpdateProgress))
 * --- to jest konieczne, bo wywołania OnTestFinish i OnTestProgress pochodzą z innego wątku, a Form nie pozwala innym wątkom dotykać swoich kontrolek
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace X2
{
    public interface IQATestLaunchPoint
    {
        DataTable GetTestPlanAsDataTable(); //test plan przeniesiony do test stuff
        IQATestLaunchPoint GetLaunchPoint();
        QATestStuff GetTestStuff();
        void OnTestProgress();
        void OnTestFinish();
    }

}
