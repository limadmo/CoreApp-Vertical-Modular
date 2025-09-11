/**
 * Arquivo principal da aplicação CoreApp Frontend
 * Configura providers, temas e roteamento
 */
import React from 'react'
import ReactDOM from 'react-dom/client'
import { App } from './App'
import '@mantine/core/styles.css'
import '@mantine/notifications/styles.css'
import '@mantine/dates/styles.css'
import './styles/globals.css'
import './styles/verticals/padaria.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)