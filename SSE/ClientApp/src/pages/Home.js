import React, { useEffect, useState, useContext } from 'react';
import { Redirect } from 'react-router-dom';
import { AuthContext } from '../providers/Auth';
import Header from '../components/Header';
import TodoForm from '../components/TodoForm';
import CreatorList from '../components/CreatorList';
import WorkerList from '../components/WorkerList';

import styles from './Home.module.css';

let source;

function Home() {
  const [todoList, setTodoList] = useState([]);
  const [rooms, setRooms] = useState([]);
  const auth = useContext(AuthContext);
  const { currentRoom, user } = auth;

  // Load rooms
  useEffect(() => {
    fetch('api/Rooms', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
      .then((response) => response.json())
      .then((json) => {
        setRooms(json);
      });
  }, []);

  // Set connection to SSE
  useEffect(() => {
    if (user) {
      if (source) {
        source.close();
      }

      source = new EventSource(`api/Sse/${currentRoom}/${user.userName}`);
    }
  }, [user]);

  // Add SSE connection event handlers
  useEffect(() => {
    const handleEventSourceOpen = (event) => {
      console.log('OPEN');
    };

    const handleMessageReceive = (event) => {
      const eventData = JSON.parse(event.data);
      const { payload } = eventData;

      switch (eventData.event) {
        case 'init':
        case 'update-room': {
          setTodoList(eventData.payload);
          break;
        }
        case 'create': {
          const todoListCopy = [...todoList];
          todoListCopy.push(payload);
          setTodoList(todoListCopy);
          break;
        }
        case 'update':
        case 'toggle': {
          const todoListCopy = [...todoList];
          const index = todoListCopy.findIndex((t) => t.id === payload.id);
          const updatedTodo = { ...todoListCopy[index], ...payload };

          todoListCopy[index] = updatedTodo;

          setTodoList(todoListCopy);
          break;
        }
        case 'delete': {
          const deleted = JSON.parse(payload);
          const todoListCopy = [...todoList];
          setTodoList(todoListCopy.filter((todo) => todo.id !== deleted.id));
          break;
        }
        default:
          break;
      }
    };

    const handleEventSourceError = (event) => {};

    if (source) {
      source.addEventListener('message', handleMessageReceive, false);

      source.addEventListener('open', handleEventSourceOpen, false);

      source.addEventListener('error', handleEventSourceError, false);
    }
    return () => {
      if (source) {
        source.removeEventListener('message', handleMessageReceive, false);

        source.removeEventListener('open', handleEventSourceOpen, false);

        source.removeEventListener('error', handleEventSourceError, false);
      }
    };
  }, [todoList]);

  if (!auth.user) {
    return <Redirect to="/login" />;
  }

  return (
    <>
      <Header title={auth.user.isCreator ? 'Submitted Tasks' : 'Task List'} rooms={rooms} currentRoom={currentRoom} />
      <main className={styles.main}>
        {auth.user.isCreator ? (
          <>
            <TodoForm />
            <CreatorList
              todoList={todoList}
              onDelete={async (todo) => {
                await fetch(`api/Sse/${auth.currentRoom}?id=${todo.id}`, {
                  method: 'DELETE',
                  headers: {
                    'Content-Type': 'application/json',
                  },
                });
              }}
            />
          </>
        ) : (
          <WorkerList
            todoList={todoList}
            onToggle={async (id) => {
              await fetch(`api/Sse/${auth.currentRoom}/Toggle?id=${id}`, {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json',
                  // 'Content-Type': 'application/x-www-form-urlencoded',
                },
              });
            }}
          />
        )}
      </main>
    </>
  );
}

export default Home;
