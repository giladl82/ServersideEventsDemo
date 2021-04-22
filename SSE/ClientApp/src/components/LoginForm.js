import React, { useContext, useState } from 'react';
import { AuthContext } from '../providers/Auth';

import styles from './LoginForm.module.css';
console.log(styles);

export default function LoginForm() {
  const auth = useContext(AuthContext);

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  return (
    <form
      className={styles.login}
      onSubmit={async (event) => {
        event.preventDefault();
        try {
          const user = await fetch(`/api/auth/login?username=${username}&password=${password}`)
            .then((response) => {
              if (response.status !== 200) {
                throw new Error();
              }

              return response.json();
            })
            .then((json) => json);

          auth.setUser(user);
        } catch (e) {
          alert('Bad Login !');
        }
      }}
    >
      <label>User name:</label>
      <input
        type="text"
        placeholder="User name"
        value={username}
        onChange={(event) => {
          setUsername(event.target.value);
        }}
      />
      <label>Password:</label>
      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={(event) => {
          setPassword(event.target.value);
        }}
      />
      <button data-title="Login">Login</button>
    </form>
  );
}
