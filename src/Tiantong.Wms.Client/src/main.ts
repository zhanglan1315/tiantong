import Vue from 'vue'
import App from './views/App.vue'
import store from './store'
import notify from './providers/notify'
import router from './providers/router'
import confirm from './providers/confirm'
import directives from './providers/directives'

Vue.config.productionTip = process.env.VUE_APP_API_URL_BASE

Vue.use(directives)
Vue.use(notify)
Vue.use(confirm)

new Vue({
  router,
  store,
  render: (h) => h(App),
}).$mount('#app')
