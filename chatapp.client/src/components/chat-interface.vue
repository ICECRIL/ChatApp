<template>
  <div class="chat-root">
    <div class="chat-container">
      <header class="chat-header">
        <img src="@/assets/logo.png" alt="Logo" class="chat-logo" />
        <h1>Чат</h1>
      </header>
      <main class="chat-main">
        <div class="chat-messages" ref="messagesEnd">
          <div v-for="(message, index) in messages" :key="index" class="chat-bubble">
            <div class="chat-bubble-header">
              <span class="chat-user">{{ message.user }}</span>
              <span class="chat-time">{{ formatTime(message.createdAt) }}</span>
              <button class="chat-delete-message-btn" @click="deleteMessage(message.id)" title="Удалить сообщение">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none">
                  <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"
                    fill="currentColor" />
                </svg>
              </button>
            </div>
            <div class="chat-text">{{ message.text }}</div>
          </div>
        </div>
      </main>
      <footer class="chat-footer">
        <input class="chat-user-input" type="text" v-model="userInput" placeholder="Ваше имя" maxlength="20" />
        <input class="chat-message-input" type="text" v-model="messageInput" placeholder="Введите сообщение..."
          maxlength="4000" @keyup.enter="sendMessage" />
        <button class="chat-clear-btn" @click="clearHistory" title="Очистить историю">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
            <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"
              fill="currentColor" />
          </svg>
        </button>
        <button class="chat-send-btn" @click="sendMessage" :disabled="!canSend">
          <svg width="22" height="22" fill="none" viewBox="0 0 24 24">
            <path d="M3 20v-6l13-2-13-2V4l18 8-18 8z" fill="currentColor" />
          </svg>
        </button>
      </footer>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onBeforeUnmount, watch, nextTick } from 'vue';
import * as signalR from '@microsoft/signalr';

export default {
  setup() {
    const userInput = ref('');
    const messageInput = ref('');
    const messages = ref([]);
    const messagesEnd = ref(null);
    let connection = null;

    const formatTime = (iso) => {
      if (!iso) return '';
      const date = new Date(iso);
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    const canSend = ref(false);
    watch([userInput, messageInput], () => {
      canSend.value = !!userInput.value.trim() && !!messageInput.value.trim();
    });

    const startConnection = async () => {
      connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7270/chatHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

      connection.on("ReceiveMessage", (message) => {
        messages.value.push({
          id: message.id,
          user: message.userName,
          text: message.content,
          createdAt: message.createdAt
        });
      });

      connection.on("ReceiveHistory", (history) => {
        messages.value = history
          .reverse()
          .map(msg => ({
            id: msg.id,
            user: msg.userName,
            text: msg.content,
            createdAt: msg.createdAt
          }));
      });

      connection.on("MessageDeleted", (messageId) => {
        messages.value = messages.value.filter(msg => msg.id !== messageId);
      });

      connection.on("HistoryCleared", () => {
        messages.value = [];
      });

      try {
        await connection.start();
        await connection.invoke("GetHistory");
      } catch (err) {
        console.error("Ошибка подключения:", err);
        setTimeout(startConnection, 5000);
      }
    };

    const clearHistory = async () => {
      if (connection && confirm('Вы уверены, что хотите очистить всю историю чата?')) {
        try {
          await connection.invoke("DeleteAllMessages");
        } catch (err) {
          console.error("Ошибка очистки истории:", err);
        }
      }
    };

    const deleteMessage = async (messageId) => {
      if (connection && confirm('Удалить это сообщение?')) {
        try {
          await connection.invoke("DeleteMessage", messageId);
        } catch (err) {
          console.error("Ошибка удаления сообщения:", err);
        }
      }
    };

    const sendMessage = async () => {
      if (connection && canSend.value) {
        try {
          await connection.invoke("SendMessage", userInput.value.trim(), messageInput.value.trim());
          messageInput.value = '';
          await nextTick();
          scrollToBottom();
        } catch (err) {
          console.error("Ошибка отправки:", err);
        }
      }
    };

    const scrollToBottom = () => {
      if (messagesEnd.value) {
        messagesEnd.value.scrollTop = messagesEnd.value.scrollHeight;
      }
    };

    watch(messages, () => {
      scrollToBottom();
    }, { deep: true });

    onMounted(() => {
      startConnection();
      scrollToBottom();
    });

    onBeforeUnmount(() => {
      if (connection) connection.stop();
    });

    return {
      userInput,
      messageInput,
      messages,
      sendMessage,
      clearHistory,
      deleteMessage,
      messagesEnd,
      canSend,
      formatTime
    };
  }
};
</script>

<style scoped>
.chat-root {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  background-color: #faf6f000;
  padding: 20px;
}

.chat-container {
  width: 100%;
  max-width: 480px;
  height: 780px;
  background: #fffdfa;
  border-radius: 22px;
  box-shadow: 0 8px 32px 0 rgba(180, 150, 100, 0.10);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1.5px solid #f3e6d2;
}

.chat-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 22px 28px 12px 28px;
  background: #fff8ee;
  border-bottom: 1.5px solid #f3e6d2;
  flex-shrink: 0;
}

.chat-logo {
  width: 36px;
  height: 36px;
}

.chat-header h1 {
  font-size: 1.35rem;
  color: #b89b72;
  margin: 0;
  font-weight: 700;
  letter-spacing: 1px;
}

.chat-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.chat-messages {
  flex: 1;
  padding: 18px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.chat-bubble {
  align-self: flex-start;
  background: #fff8ee;
  border-radius: 18px 18px 18px 6px;
  padding: 12px 18px;
  max-width: 80%;
  box-shadow: 0 2px 8px 0 rgba(200, 170, 120, 0.07);
  border: 1px solid #f3e6d2;
  position: relative;
  word-break: break-word;
}

.chat-bubble-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 4px;
  gap: 8px;
}

.chat-user {
  font-weight: bold;
  color: #b89b72;
  margin-right: auto;
  font-size: 1rem;
}

.chat-time {
  color: #c2a77b;
  font-size: 0.75rem;
  font-style: italic;
  white-space: nowrap;
}

.chat-text {
  color: #7c6a4d;
  font-size: 1rem;
  line-height: 1.4;
  white-space: pre-line;
}

.chat-footer {
  display: flex;
  gap: 8px;
  padding: 18px;
  background: #fff8ee;
  border-top: 1.5px solid #f3e6d2;
  align-items: center;
  flex-shrink: 0;
}

.chat-user-input {
  width: 100px;
  min-width: 70px;
  background: #fdf6ee;
  border: 1.5px solid #f3e6d2;
  border-radius: 8px;
  padding: 8px 10px;
  font-size: 1rem;
  color: #a67c52;
  transition: border 0.2s, box-shadow 0.2s;
  box-shadow: 0 1px 4px 0 rgba(245, 222, 179, 0.08);
}

.chat-user-input:focus {
  border: 1.5px solid #e6cfa7;
  background: #f9f3e7;
  outline: none;
  box-shadow: 0 2px 8px 0 rgba(245, 222, 179, 0.13);
}

.chat-user-input::placeholder {
  color: #c2a77b;
  opacity: 1;
  font-style: italic;
}

.chat-message-input {
  flex: 1;
  border: 1.5px solid #f3e6d2;
  border-radius: 8px;
  padding: 8px 12px;
  font-size: 1rem;
  background: #fff8ee;
  color: #7c6a4d;
  outline: none;
  transition: border 0.2s;
}

.chat-message-input:focus {
  border: 1.5px solid #e6cfa7;
  background: #f9f3e7;
}

.chat-send-btn {
  background: #b89b72;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 8px 18px;
  font-size: 1.1rem;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 40px;
  min-width: 40px;
}

.chat-send-btn:disabled {
  background: #e6cfa7;
  color: #fff8ee;
  cursor: not-allowed;
  opacity: 0.7;
}

.chat-send-btn:hover:not(:disabled) {
  background: #a67c52;
  color: #fff;
}

.chat-clear-btn {
  background: #ff6b6b;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 12px;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 40px;
  min-width: 40px;
}

.chat-clear-btn:hover {
  background: #ff5252;
}

.chat-delete-message-btn {
  background: none;
  border: none;
  color: #c2a77b;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  opacity: 0;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chat-bubble:hover .chat-delete-message-btn {
  opacity: 1;
}

.chat-delete-message-btn:hover {
  color: #ff6b6b;
  background: rgba(255, 107, 107, 0.1);
}

@media (max-width: 600px) {
  .chat-root {
    padding: 0;
    display: block;
  }

  .chat-container {
    max-width: 100vw;
    height: 100vh;
    border-radius: 0;
  }

  .chat-header,
  .chat-footer {
    padding: 15px;
  }

  .chat-messages {
    padding: 12px;
  }

  .chat-delete-message-btn {
    opacity: 1;
  }
}
</style>
