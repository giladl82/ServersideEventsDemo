import React, { useState } from 'react';
import Cookies from 'js-cookie';
export const AuthContext = React.createContext();

export const AuthProvider = ({ children }) => {
  const cookieValue = Cookies.get('auth-user');
  const [user, setUser] = useState(cookieValue ? JSON.parse(cookieValue) : null);
  const [currentRoom, setRoom] = useState('Room1');

  return (
    <AuthContext.Provider
      value={{
        user,
        currentRoom,
        token: Cookies.get('auth-token'),
        setUser: (user) => {
          setUser(user);
          if (user) {
            Cookies.set('auth-token', user.token);
            Cookies.set('auth-user', user);
          } else {
            Cookies.remove('auth-token');
            Cookies.remove('auth-user');
          }
        },
        setRoom: (room) => {
          setRoom(room);
        },
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
