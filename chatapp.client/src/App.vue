<script>
import { defineComponent } from 'vue';
import chatInterface from './components/chat-interface.vue';

export default defineComponent({
  components: {
    chatInterface
  }
})
</script>

<template>
  <div>
    <chat-interface></chat-interface>
  </div>

</template>

<style scoped></style>
