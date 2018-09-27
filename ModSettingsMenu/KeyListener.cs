using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModSettingsMenu
{
    public class KeyListener : MonoBehaviour, IUpdateSelectedHandler
    {
        private Event m_ProcessingEvent = new Event();
        private KeyCode m_LastKey = KeyCode.None;
        private Action<KeyCode, EventModifiers> keyDownCallback = null;
        private Action<KeyCode, EventModifiers> keyUpCallback = null;

        public void OnUpdateSelected(BaseEventData eventData)
        {
            while(Event.PopEvent(m_ProcessingEvent))
            {
                switch(m_ProcessingEvent.rawType)
                {
                    case EventType.KeyDown:
                    {
                        KeyDownEventHandler(m_ProcessingEvent);
                        break;
                    }

                    case EventType.KeyUp:
                    {
                        KeyUpEventHandler(m_ProcessingEvent);
                        break;
                    }
                }
            }
            eventData.Use();
        }

        public void KeyDownEventHandler(Event e)
        {
            if(e.keyCode == KeyCode.None) return;
            if(e.keyCode == m_LastKey) return;
            keyDownCallback?.Invoke(e.keyCode, e.modifiers);
        }

        public void KeyUpEventHandler(Event e)
        {
            keyUpCallback?.Invoke(e.keyCode, e.modifiers);
            m_LastKey = KeyCode.None;
        }

        public void RegisterKeyDownCallback(Action<KeyCode, EventModifiers> keyDownCallback)
        {
            this.keyDownCallback = keyDownCallback;
        }

        public void RegisterKeyUpCallback(Action<KeyCode, EventModifiers> keyUpCallback)
        {
            this.keyUpCallback = keyUpCallback;
        }
    }
}
