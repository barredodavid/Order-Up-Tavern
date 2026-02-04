using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("Portraits")]
    public Sprite tavernOwnerPortrait;

    void Start()
    {
        StartCoroutine(LaunchTutorialDelayed());
    }

    IEnumerator LaunchTutorialDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("❌ DialogueManager.Instance est NULL !");
            yield break;
        }
        
        StartTutorial();
    }

    void StartTutorial()
    {
        Debug.Log("🎓 Lancement du tutoriel de démarrage...");
        
        List<DialogueLine> tutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Bonjour ! Je suis le vieux propriétaire de cette taverne."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Je vais t'aider à ouvrir ta taverne pour la première fois !"),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Pour commencer, attrape le balai et nettoie la salle.")
        };

        if (DialogueManager.Instance != null) DialogueManager.Instance.StartDialogue(tutorial);
    }

    // --- DIALOGUES EXISTANTS ---

    public void ShowFirstCustomerTutorial()
    {
        Debug.Log("🎉 Tutoriel : Premier client");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> firstCustomer = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Regarde, ton premier client ! Va prendre un verre dans l'armoire pour le servir.")
        };
        DialogueManager.Instance.StartDialogue(firstCustomer);
    }

    public void ShowFillBeerTutorial()
    {
        Debug.Log("🍺 Tutoriel : aller au tonneau");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> fillTutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Parfait ! Maintenant remplis ton verre de bière qui se trouve dans le fût.")
        };
        DialogueManager.Instance.StartDialogue(fillTutorial);
    }

    public void ShowServeCustomerTutorial()
    {
        Debug.Log("🍺 Tutoriel : servir le client");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> serveTutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Super ! Le verre est plein. Maintenant va servir le client qui attend au bar !")
        };
        DialogueManager.Instance.StartDialogue(serveTutorial);
    }

    // --- LE TUTO DU VERRE SALE (Le début) ---
    public void ShowDirtyGlassTutorial()
    {
        Debug.Log("🧼 Tutoriel : Verre sale repéré");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> dirtyTutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Regarde ! Le client a laissé son verre sale. Prends-le, on ne peut pas laisser ça là.")
        };
        DialogueManager.Instance.StartDialogue(dirtyTutorial);
    }

    // QUAND TU AS LE VERRE EN MAIN ---
    public void ShowPutInBucketTutorial()
    {
        Debug.Log("🪣 Tutoriel : Mettre dans le bac");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> bucketLines = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Bien ! Ne laisse pas ça traîner. Mets ce verre sale dans le bac de lavage.")
        };
        DialogueManager.Instance.StartDialogue(bucketLines);
    }

    //  QUAND LE VERRE EST DANS LE BAC
    public void ShowWashMechanicTutorial()
    {
        Debug.Log("🧼 Tutoriel : Mécanique de lavage");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> washLines = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Parfait. Maintenant, retrousse tes manches et frotte ce verre jusqu'à ce qu'il brille !")
        };
        DialogueManager.Instance.StartDialogue(washLines);
    }


    public void ShowFirstCustomerServedTutorial()
    {
        Debug.Log("🎉 Tutoriel : Premier client servi");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> congratsTutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Bravo ! Tu as servi ton premier client ! Continue comme ça !")
        };
        DialogueManager.Instance.StartDialogue(congratsTutorial);
    }

    public void ShowEndTutorialDialogue()
    {
        Debug.Log("🎓 Tutoriel terminé !");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> endTutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Beau travail ! Ce verre est comme neuf."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Tu as compris les bases : Servir, Encaisser, Nettoyer.")
        };
        DialogueManager.Instance.StartDialogue(endTutorial);
    }

    public void ShowShopTutorial()
    {
        Debug.Log("📖 Tutoriel : Le Shop");
        if (DialogueManager.Instance == null) return;

        List<DialogueLine> shopTutorial = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Bien ! Avant d'ouvrir, jette un œil au Livre d'améliorations."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "C'est là que tu pourras acheter des améliorations avec ton or."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Prends ton temps pour te préparer. Quand tu te sens prêt, ouvre les portes et laisse entrer les clients !")
        };
        DialogueManager.Instance.StartDialogue(shopTutorial);
    }

    public void ShowInspectorTutorial()
    {
        Debug.Log("🕵️ Tuto : Inspecteur Aléatoire");

        if (DialogueManager.Instance == null) return;

        List<DialogueLine> inspectorLines = new List<DialogueLine>
        {
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Bien, tu as vu les améliorations."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Dernière chose : Fais attention à l'Inspecteur d'Hygiène."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Il peut débarquer à N'IMPORTE QUEL MOMENT de la journée pour vérifier la propreté."),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Si ta taverne est sale quand il rentre : Amende immédiate !"),
            new DialogueLine("Vieux Barman", tavernOwnerPortrait, "Alors garde toujours un œil sur les taches. C'est tout, tu peux ouvrir !")
        };

        DialogueManager.Instance.StartDialogue(inspectorLines);
    }
}