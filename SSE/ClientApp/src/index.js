import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom';
import registerServiceWorker from './registerServiceWorker';
import { AuthProvider } from './providers/Auth';

import Home from './pages/Home';
import Login from './pages/Login';

import './index.css';

const rootElement = document.getElementById('root');

ReactDOM.render(
  <AuthProvider>
    <Router>
      <Switch>
        <Route exact path="/" component={Home} />
        <Route exact path="/login" component={Login} />
      </Switch>
    </Router>
  </AuthProvider>,
  rootElement
);

registerServiceWorker();
