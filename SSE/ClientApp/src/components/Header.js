import React, { useContext } from 'react';
import { AuthContext } from '../providers/Auth';

import styles from './Header.module.css';

export default function Header({ title, rooms, currentRoom }) {
  const auth = useContext(AuthContext);

  return (
    <header className={styles.header}>
      <h1>{title}</h1>
      {auth.user && (
        <div>
          <div>
            Logged in as: <strong>{auth.user.userName}</strong>
            <a
              href="/"
              onClick={(event) => {
                event.preventDefault();
                auth.setUser(null);
              }}
            >
              Log out
            </a>
          </div>
          <div>
            <select value={currentRoom} onChange={event => {
              const {value} = event.target;
              auth.setRoom(value);

              fetch(`api/Sse/ChangeRoom/${auth.currentRoom}/${value}`, {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json',
                }
              });
            }}>
              {rooms.map(room => (
                <option key={room} value={room} >{room}</option>
              ))}
            </select>
          </div>
        </div>
      )}
    </header>
  );
}
