import AccountForm from "../Forms/AccountForm";
import AchievementForm from "../Forms/AchievementForm";
import TransactionForm from "../Forms/TransactionForm";
import { useContext, useEffect } from "react";
import { UserContext } from "../../context/userContext";
import { useNavigate } from "react-router-dom";
import { fetchData } from "../../service/connectionService";
import { SnackbarContext } from "../../context/snackbarContext";
import SnackBar from "../Snackbar/Snackbar";
import "./Home.css";

const Home = () => {
  const { currentUser } = useContext(UserContext);
  const { snackbar, setSnackbar } = useContext(SnackbarContext);
  const navigate = useNavigate();

  useEffect(() => {
    setTimeout(() => {
      setSnackbar({
        open: false,
        message: "",
        type: "",
      });
    }, 6000);
  }, [setSnackbar]);

  const handleLogout = async () => {
    const response = await fetchData(null, "/User/Logout", "POST");
    if (response.ok) {
      window.location.reload();
      return;
    } else {
      setSnackbar({
        open: true,
        message: response.message,
        type: "error",
      });
    }
  };

  const handleLogging = () => {
    if (currentUser) {
      handleLogout();
      return;
    } else {
      navigate("/authentication");
      return;
    }
  };

  return (
    <div className="container mt-4">
      <SnackBar
        {...snackbar}
        setOpen={() => setSnackbar({ ...snackbar, open: false })}
      />
      <div className="row justify-content-between">
        <div className="col-md-5 mx-2">
          <AccountForm />
          <AchievementForm />
          <TransactionForm />
        </div>
        <div className="my-8 col-md-1">
          <button onClick={handleLogging} className="btn btn-lg btn-info">
            {currentUser ? "Logout" : "Login"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default Home;