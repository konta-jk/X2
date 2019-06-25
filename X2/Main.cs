/*
 * https://saucelabs.com/resources/articles/getting-started-with-webdriver-in-c-using-visual-studio
 * 
 * problemy
 * - zmiany psują xpath, a więc konfiguracje testów; może inna strategia wyszukiwania elementów?
 * 
 * todo
 * 
 * - więcej przypadków testowych i rozbudowa ElementOperation
 * - sprawdzić obsługę: akcji na wierszu z rozwijanego menu, ...
 * - dobrze byłoby wydłuzyć sleep po ostatnim kroku
 * - klasa main jest zbędna
 * 
 * todo po rozmowie z PK: 
 * - ogólnie kodowanie PK: używać zmiennych lokalnych o ile tylko się da, nieważne że ich będzie pierdyliard
 * - VS PK: da się ustawić warunek w punkcie przerwania
 * - VS PK: jest jakieś szybkie testowanie funkcji - okienko do tego
 * - wtyczki pozwalające, być może, szybciej uzyskac xpath:
 * -- https://chrome.google.com/webstore/detail/xpath-finder/ihnknokegkbpmofmafnkoadfjkhlogph?hl=en
 * - straszliwe lamerstwo z przekazywaniem rezultatu do wątku z ui
 * -- zastąpić eventem (wątek z selenium ma poinformować, że skończyl i przekazać result w params, od biedy results brane z instancji complete test)
 * - dorobić akcję czekaj, bo czasami trzeba poczekac na wygenerowanie zadania; albo jak PK mówił, dodać opcjonalny parametr wait po kroku w excellu
 * 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    class Main
    {
        //
    }
}
