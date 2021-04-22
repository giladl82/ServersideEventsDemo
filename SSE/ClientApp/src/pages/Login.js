import React, { useContext } from 'react';
import { Redirect } from 'react-router-dom';
import { AuthContext } from '../providers/Auth';
import Header from '../components/Header';
import LoginForm from '../components/LoginForm';

function Login() {
  const auth = useContext(AuthContext);

  if (auth.user) {
    return <Redirect to="/" />;
  }

  return (
    <main>
      <Header title="Login" />
      <LoginForm />
    </main>
  );
}

export default Login;
