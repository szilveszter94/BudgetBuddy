/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect } from "react";
import { fetchData } from "../../../service/connectionService";
import SnackBar from "../../Snackbar/Snackbar";
import Loading from "../../Loading/Loading";
import { useNavigate, useParams } from "react-router-dom";

const sampleAchievement = {
  name: "",
  description: "",
};

const AchievementCreator = () => {
  const [achievement, setAchievement] = useState(sampleAchievement);
  const [loading, setLoading] = useState(false);
  const { id } = useParams();
  const navigate = useNavigate();
  const [localSnackbar, setLocalSnackbar] = useState({
    open: false,
    message: "",
    type: "",
  });

  useEffect(() => {
    if (id) {
      fetchAchievementData();
    }
  }, []);

  const fetchAchievementData = async () => {
    try {
      setLoading(true);
      const response = await fetchData(null, `/Achievement/${id}`, "GET");
      if (response.ok) {
        setAchievement(response.data.data);
        setLocalSnackbar({
          open: true,
          message: response.message,
          type: "success",
        });
      } else {
        setAchievement(sampleAchievement);
        setLocalSnackbar({
          open: true,
          message: response.message,
          type: "error",
        });
      }
    } catch (error) {
      setAchievement(sampleAchievement);
      setLocalSnackbar({
        open: true,
        message: "Server not responding.",
        type: "error",
      });
    }
    setLoading(false);
  };

  const handleAchievementChange = (e) => {
    const key = e.target.name;
    const value = e.target.value;
    setAchievement({ ...achievement, [key]: value });
  };

  const handleBack = () => {
    navigate("/");
  };

  const handleCreateAchievement = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const response = await fetchData(
        achievement,
        id ? "/Achievement/update" : "/Achievement/add",
        id ? "PATCH" : "POST"
      );
      if (response.ok) {
        setLocalSnackbar({
          open: true,
          message: response.message,
          type: "success",
        });
      } else {
        setLocalSnackbar({
          open: true,
          message: response.message,
          type: "error",
        });
      }
    } catch (error) {
      setLocalSnackbar({
        open: true,
        message: "Server not responding.",
        type: "error",
      });
    }
    setLoading(false);
    setAchievement(sampleAchievement);
  };

  if (loading) {
    return <Loading />;
  }

  return (
    <div className="container mt-5">
      <SnackBar
        {...localSnackbar}
        setOpen={() => setLocalSnackbar({ ...localSnackbar, open: false })}
      />
      <h1>{id ? "Update achievement" : "Create new achievement:"}</h1>
      <form onSubmit={handleCreateAchievement}>
        <label className="form-label mb-3" htmlFor="name">
          Name
        </label>
        <input
          onChange={handleAchievementChange}
          className="form-control mb-3"
          required
          value={achievement.name}
          type="text"
          id="name"
          name="name"
          placeholder="Enter the name of the achievement"
        />
        <label className="form-label mb-3" htmlFor="description">
          Description
        </label>
        <input
          onChange={handleAchievementChange}
          className="form-control mb-3"
          value={achievement.description}
          required
          type="text"
          id="description"
          name="description"
          placeholder="Enter the description of the achievement"
        />
        <div>
          <div className="mb-5">
            <button className="btn btn-info ms-4" type="submit">
              Submit
            </button>
            <button onClick={handleBack} className="btn btn-dark ms-4">
              Back
            </button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default AchievementCreator;
